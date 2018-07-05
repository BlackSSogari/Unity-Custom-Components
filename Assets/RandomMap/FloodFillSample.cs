using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloodFillSample : MonoBehaviour {

	public Image colorFillMap;
	public GameObject tilePrefab;
	public GameObject obstaclePrefab;
	
	[Range(0, 1)]
	public float outlinePercent;

    private void Start()
    {
        InvokeRepeating("StartFloodFill", 1f, 3f);
    }

    public void StartFloodFill()
	{
		GenerateMap();
		GenerateColorFillMap();
	}

	#region Generate Color Fill Map

	public struct ColorNode
	{
		public int x;
		public int y;
		public Image img;
		public bool isVisit;

		public ColorNode(int _x, int _y, Image _img, bool _isVisit)
		{
			x = _x;
			y = _y;
			img = _img;
			isVisit = _isVisit;
		}
	}

	public bool isUseDFS = true;
	
	private Vector2 colorMapSize = new Vector2(9, 9);	
	ColorNode[,] allColorImageMap;
	ColorNode colorMapCentre;
	public void GenerateColorFillMap()
	{
		allColorImageMap = new ColorNode[(int)colorMapSize.x, (int)colorMapSize.y];		
		
		List<Coord> innerBlockCode = new List<Coord>();
		innerBlockCode.Add(new Coord(4, 7));
		innerBlockCode.Add(new Coord(4, 6));
		innerBlockCode.Add(new Coord(3, 5));
		innerBlockCode.Add(new Coord(1, 4));
		innerBlockCode.Add(new Coord(2, 4));

		innerBlockCode.Add(new Coord(7, 4));
		innerBlockCode.Add(new Coord(6, 4));
		innerBlockCode.Add(new Coord(5, 3));
		innerBlockCode.Add(new Coord(4, 1));
		innerBlockCode.Add(new Coord(4, 2));

		for (int x = 0; x< colorMapSize.x; x++)
		{
			for (int y = 0; y< colorMapSize.y; y++)
			{
				Vector3 tilePosition = CoordToImagePosition(x, y);
				GameObject newTile = new GameObject(string.Format("Tile_{0}_{1}", x, y), typeof(Image));
				Image newTileImage = newTile.GetComponent<Image>();
				ColorNode newNode = new ColorNode(x, y, newTileImage, false);
				allColorImageMap[x, y] = newNode;
				newTile.transform.SetParent(colorFillMap.transform);
				newTile.transform.localPosition = tilePosition;
				newTile.transform.localScale = Vector3.one * (1 - outlinePercent);

				if (x == (int)colorMapSize.x / 2 && y == (int)colorMapSize.y / 2)
				{
					colorMapCentre = newNode;
				}

				if (x == 0 || x == (colorMapSize.x - 1) || y == 0 || y == (colorMapSize.y - 1) || IsBlockCoord(innerBlockCode, x, y))
				{
					newTileImage.color = Color.black;
					newNode.isVisit = true;
				}
			}
		}

		if (isUseDFS)
		{
			//DFS_FloodFill_Recursive(colorMapCentre, Color.white, Color.red);
			StartCoroutine(CoDFS_FloodFill_Recursive(colorMapCentre, Color.white, Color.red));
		}
		else
		{
			//BFS_FloodFill_Queue(colorMapCentre, Color.white, Color.red);
			StartCoroutine(CoBFS_FloodFill_Queue(colorMapCentre, Color.white, Color.red));
		}
	}
		
	private bool IsBlockCoord(List<Coord> blockList, int _x, int _y)
	{
		int idx = blockList.FindIndex(c => c.x == _x && c.y == _y);
		if (idx < 0)
			return false;

		return true;
	}

	Vector3 CoordToImagePosition(int x, int y)
	{
		return new Vector3((-colorMapSize.x / 2 + 0.5f + x) * 100, (-colorMapSize.y / 2 + 0.5f + y) * 100, 0);
	}

	/// <summary>
	/// DFS(Depth first search) 그래프탐색 방식의 알고리즘을 사용하여 재귀적으로 처리한다.
	/// The above code visits each and every cell of a matrix of size n×m starting with some source cell.Time Complexity of above algorithm is O(n×m).
	/// </summary>	
	private void DFS_FloodFill_Recursive(ColorNode node, Color targetColor, Color replaceColor)
	{
		if (node.x < 0 || node.y < 0)
			return;
		if (node.x >= allColorImageMap.GetLength(0) || node.y >= allColorImageMap.GetLength(1))
			return;
				
		if (node.isVisit)
			return;

		// If the color of node is not equal to target-color, return.
		if (node.img.color != targetColor)
			return;

		// Set the color of node to replacement-color.
		node.img.color = replaceColor;

		bool wikipedia_algorithm = true;
		if (wikipedia_algorithm)
		{
			// south
			int neighbourX = node.x;
			int neighbourY = node.y - 1;
			DFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// north
			neighbourX = node.x;
			neighbourY = node.y + 1;
			DFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// west
			neighbourX = node.x - 1;
			neighbourY = node.y;
			DFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// east
			neighbourX = node.x + 1;
			neighbourY = node.y;
			DFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
		}
		else
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					// 인접한 8개 타일을 순환한다.
					int neighbourX = node.x + x;
					int neighbourY = node.y + y;

					// 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
					if (x == 0 || y == 0)
					{
						DFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
					}
				}
			}
		}
	}

	IEnumerator CoDFS_FloodFill_Recursive(ColorNode node, Color targetColor, Color replaceColor)
	{
		if (node.x < 0 || node.y < 0)
			yield break;
		if (node.x >= allColorImageMap.GetLength(0) || node.y >= allColorImageMap.GetLength(1))
			yield break;
		if (node.isVisit)
			yield break;
		if (node.img.color != targetColor)
			yield break;

		node.img.color = replaceColor;
		yield return new WaitForSeconds(0.1f);

		bool wikipedia_algorithm = true;
		if (wikipedia_algorithm)
		{
			// south
			int neighbourX = node.x;
			int neighbourY = node.y - 1;
			yield return CoDFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// north
			neighbourX = node.x;
			neighbourY = node.y + 1;
			yield return CoDFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// west
			neighbourX = node.x - 1;
			neighbourY = node.y;
			yield return CoDFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
			// east
			neighbourX = node.x + 1;
			neighbourY = node.y;
			yield return CoDFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
		}
		else
		{
			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					// 인접한 8개 타일을 순환한다.
					int neighbourX = node.x + x;
					int neighbourY = node.y + y;

					// 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
					if (x == 0 || y == 0)
					{
						yield return CoDFS_FloodFill_Recursive(allColorImageMap[neighbourX, neighbourY], targetColor, replaceColor);
					}
				}
			}
		}
	}

	private void BFS_FloodFill_Queue(ColorNode node, Color targetColor, Color replaceColor)
	{
		if (node.x < 0 || node.y < 0)
			return;
		if (node.x >= allColorImageMap.GetLength(0) || node.y >= allColorImageMap.GetLength(1))
			return;
		if (node.isVisit)
			return;
		// If the color of node is not equal to target-color, return.
		if (node.img.color != targetColor)
			return;
		
		Queue<ColorNode> queue = new Queue<ColorNode>();
		
		// Set the color of node to replacement-color.
		node.img.color = replaceColor;
		// Add node to the end of Q.
		queue.Enqueue(node);
		// While Q is not empty
		while (queue.Count > 0)
		{
			ColorNode n = queue.Dequeue();

			bool wikipedia_algorithm = true;
			if (wikipedia_algorithm)
			{
				// west
				int neighbourX = n.x - 1;
				int neighbourY = n.y;
				ColorNode neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
				}

				// east
				neighbourX = n.x + 1;
				neighbourY = n.y;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
				}

				// north
				neighbourX = n.x;
				neighbourY = n.y + 1;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
				}

				// south
				neighbourX = n.x;
				neighbourY = n.y - 1;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
				}
			}
			else
			{
				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						// 인접한 8개 타일을 순환한다.
						int neighbourX = n.x + x;
						int neighbourY = n.y + y;

						// 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
						if (x == 0 || y == 0)
						{
							ColorNode neighbourNode = allColorImageMap[neighbourX, neighbourY];
							if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
							{
								neighbourNode.img.color = replaceColor;
								queue.Enqueue(neighbourNode);
							}
						}
					}
				}
			}
		}
	}

	IEnumerator CoBFS_FloodFill_Queue(ColorNode node, Color targetColor, Color replaceColor)
	{
		if (node.x < 0 || node.y < 0)
			yield break;
		if (node.x >= allColorImageMap.GetLength(0) || node.y >= allColorImageMap.GetLength(1))
			yield break;
		if (node.isVisit)
			yield break;
		// If the color of node is not equal to target-color, return.
		if (node.img.color != targetColor)
			yield break;

		Queue<ColorNode> queue = new Queue<ColorNode>();

		// Set the color of node to replacement-color.
		node.img.color = replaceColor;
		// Add node to the end of Q.
		queue.Enqueue(node);
		// While Q is not empty
		while (queue.Count > 0)
		{
			ColorNode n = queue.Dequeue();

			bool wikipedia_algorithm = true;
			if (wikipedia_algorithm)
			{
				// west
				int neighbourX = n.x - 1;
				int neighbourY = n.y;
				ColorNode neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
					yield return new WaitForSeconds(0.1f);
				}

				// east
				neighbourX = n.x + 1;
				neighbourY = n.y;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
					yield return new WaitForSeconds(0.1f);
				}

				// north
				neighbourX = n.x;
				neighbourY = n.y + 1;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
					yield return new WaitForSeconds(0.1f);
				}

				// south
				neighbourX = n.x;
				neighbourY = n.y - 1;
				neighbourNode = allColorImageMap[neighbourX, neighbourY];
				if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
				{
					neighbourNode.img.color = replaceColor;
					queue.Enqueue(neighbourNode);
					yield return new WaitForSeconds(0.1f);
				}
			}
			else
			{
				for (int x = -1; x <= 1; x++)
				{
					for (int y = -1; y <= 1; y++)
					{
						// 인접한 8개 타일을 순환한다.
						int neighbourX = n.x + x;
						int neighbourY = n.y + y;

						// 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
						if (x == 0 || y == 0)
						{
							ColorNode neighbourNode = allColorImageMap[neighbourX, neighbourY];
							if (!neighbourNode.isVisit && neighbourNode.img.color == targetColor)
							{
								neighbourNode.img.color = replaceColor;
								queue.Enqueue(neighbourNode);
								yield return new WaitForSeconds(0.1f);
							}
						}
					}
				}
			}
		}
	}

	#endregion
	//==========================================================================================
	#region Generate Accessible Map

	[Range(0, 1)]
	public float obstaclePercent;

	List<Coord> allTileCoords;
	Queue<Coord> shuffledTileCoords;

	public int seed = 10;

	private Vector2 mapSize = new Vector2(10, 10);

	Coord mapCentre;
    bool isRandomSeed = false;

	public void GenerateMap()
	{
        if (seed == 10)
            isRandomSeed = true;

        if (isRandomSeed)
            seed = Random.Range(1, 100);

		allTileCoords = new List<Coord>();
		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				allTileCoords.Add(new Coord(x, y));
			}
		}
		shuffledTileCoords = new Queue<Coord>(FisherYatesShuffle.ShuffleArray(allTileCoords.ToArray(), seed));
		mapCentre = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);


		string holderName = "Generated Map";
		if (transform.Find(holderName))
		{
			DestroyImmediate(transform.Find(holderName).gameObject);
		}

		Transform mapHolder = new GameObject(holderName).transform;
		mapHolder.parent = transform;
		mapHolder.transform.localPosition = new Vector3(-4.2f, 0, 0);

		for (int x = 0; x < mapSize.x; x++)
		{
			for (int y = 0; y < mapSize.y; y++)
			{
				Vector3 tilePosition = CoordToPosition(x, y);
				GameObject newTile = Instantiate(tilePrefab, mapHolder);
				newTile.name = string.Format("Tile_{0}_{1}", x, y);
				newTile.transform.localPosition = tilePosition;
				newTile.transform.localScale = Vector3.one * (1 - outlinePercent);
				newTile.transform.parent = mapHolder;
			}
		}

		bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

		int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
		int currentObstacleCount = 0;
		for (int i = 0; i < obstacleCount; i++)
		{
			Coord randomCoord = GetRandomCoord();
			obstacleMap[randomCoord.x, randomCoord.y] = true;
			currentObstacleCount++;
			if (randomCoord != mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
			{
				Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

				GameObject newObstacle = Instantiate(obstaclePrefab, mapHolder);
				newObstacle.transform.localPosition = obstaclePosition;
				newObstacle.transform.parent = mapHolder;
			}
			else
			{
				obstacleMap[randomCoord.x, randomCoord.y] = false;
				currentObstacleCount--;
			}
		}

	}

	bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
	{
		/*
		 * [Flood fill 알고리즘을 이용해서 닿지 않는 타일이 존재하는지 체크한다.]
		 * 중앙에는 장애물이 없는걸아니까, 먼저 obstacleMap의 중앙에서 부터 시작해서 밖으로 퍼져나가면서 타일을 검색해 나간다.
		 * 그리고 얼마나 장애물이 아닌 타일들이 있는지 숫자를 센다.
		 * 전체 타일수가 얼마나 되는지 알고 있는 상태에서, currentObstableCount를 이용해 장애물이 아닌 타일이 얼만큼 반드시 존재해야 하는지 알수 있다.
		 * 그래서 만약 Flood fill 알고리즘으로 얻은 값이 반드시 존재해야 하는 비장애물 타일 갯수와 다르다면
		 * Flood fill 알고리즘이 맵에 있는 모든 타일에 닿지 못했다는 뜻이다.
		 * 장애물에 막혀있단 의미이므로 맵전체가 접근 가능하다는 것이 아니며 false를 리턴한다.
		 * Flood fill 알고리즘을 사용할 때 중요한 점은
		 * 이미 살펴보았던 타일들을 표시해서, 같은 타일을 계속 또 살펴보지 않도록 하는것이다.
		 */

		bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
		Queue<Coord> queue = new Queue<Coord>();

		// 비어있는 좌표는 큐에 넣어준다.
		queue.Enqueue(mapCentre);

		// 맵 중앙은 비어있는것을 아니깐 이미 체크했다는 의미로 true값을 넣어준다.
		mapFlags[mapCentre.x, mapCentre.y] = true;

		// 접근 가능한 타일의 개수를 설정한다. 중앙은 비어있으니 기본적으로 1이다.
		int accessibleTileCount = 1;

		// Flood fill을 시작한다.
		while (queue.Count > 0)
		{
			Coord tile = queue.Dequeue();

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					// 인접한 8개 타일을 순환한다.
					int neighbourX = tile.x + x;
					int neighbourY = tile.y + y;

					// 수직, 수평만 살펴보고 싶으니, 대각선 방향은 체크하지 않는다.
					if (x == 0 || y == 0)
					{
						// 반드시 인접한 타일이 맵에 존재하는지 체크해야한다.
						if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
						{
							// 이미 체크했는지 확인하고, 장애물이 아닌지 체크한다.
							if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
							{
								// 체크했으므로 true값을 넣어준다.
								mapFlags[neighbourX, neighbourY] = true;

								// 비어있는 좌표이므로 큐에 넣어준다.
								queue.Enqueue(new Coord(neighbourX, neighbourY));

								// 접근 가능한 타일의 수를 증가시킨다.
								accessibleTileCount++;
							}
						}
					}
				}
			}
		}

		int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - currentObstacleCount);
		return targetAccessibleTileCount == accessibleTileCount;
	}

	Vector3 CoordToPosition(int x, int y)
	{
		return new Vector3(-mapSize.x / 2 + 0.5f + x, -mapSize.y / 2 + 0.5f + y, 0);
	}

	public Coord GetRandomCoord()
	{
		Coord randomCoord = shuffledTileCoords.Dequeue();
		shuffledTileCoords.Enqueue(randomCoord);
		return randomCoord;
	}

	public struct Coord
	{
		public int x;
		public int y;

		public Coord(int _x, int _y)
		{
			x = _x;
			y = _y;
		}

		public static bool operator ==(Coord c1, Coord c2)
		{
			return c1.x == c2.x && c1.y == c2.y;
		}

		public static bool operator !=(Coord c1, Coord c2)
		{
			return !(c1 == c2);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}

	#endregion
}
