using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public class FindScript : EditorWindow
{
	static int go_count = 0, components_count = 0, missing_count = 0;
	static List<string> m_ScriptList = new List<string>();

	[MenuItem("Tools/FindScript")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(FindScript));
	}

	public void OnGUI()
	{
		if (GUILayout.Button("Find Missing Scripts in selected prefabs"))
		{
			FindMissingScriptsInSelectedPrefabs();
		}
		if (GUILayout.Button("Find Missing Scripts Recursively in selected GameObjects"))
		{
			FindMissingScriptsRecursivelyInSelectedGameObjects();
		}			
		if (GUILayout.Button("Find Using Scripts Recursively in selected GameObjects"))
		{
			FindInSelected();
		}
		if (GUILayout.Button("Merge UsedScript File"))
		{
			MergeUsedScript();
		}
		if (GUILayout.Button("Find Missing Prefabs Recursively in selected GameObjects"))
		{
			FindMissingPrefabsRecursivelyInSelectedGameObjects();
		}
		if (GUILayout.Button("Disconnect Selected Prefab"))
		{
			DisconnectSelectedPrefab();
		}
	}

	#region FindMissingScripts
	private static void FindMissingScriptsInSelectedPrefabs()
	{
		GameObject[] go = Selection.gameObjects;
		int go_count = 0, components_count = 0, missing_count = 0;
		foreach (GameObject g in go)
		{
			go_count++;
			Component[] components = g.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				components_count++;
				if (components[i] == null)
				{
					missing_count++;
					string s = g.name;
					Transform t = g.transform;
					while (t.parent != null)
					{
						s = t.parent.name + "/" + s;
						t = t.parent;
					}
					Debug.Log(s + " has an empty script attached in position: " + i, g);
				}
			}
		}

		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
	}
	#endregion
	#region Find Script Recursively
	private static void FindMissingScriptsRecursivelyInSelectedGameObjects()
	{
		GameObject[] go = Selection.gameObjects;
		go_count = 0;
		components_count = 0;
		missing_count = 0;
		foreach (GameObject g in go)
		{
			FindMissingScriptsRecursivelyInSelectedGameObjectsInGO(g);
		}
		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
	}

	private static void FindMissingScriptsRecursivelyInSelectedGameObjectsInGO(GameObject g)
	{
		go_count++;
		Component[] components = g.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			components_count++;
			if (components[i] == null)
			{
				missing_count++;
				string s = g.name;
				Transform t = g.transform;
				while (t.parent != null)
				{
					s = t.parent.name + "/" + s;
					t = t.parent;
				}
				Debug.Log(s + " has an empty script attached in position: " + i, g);
			}
		}
		// Now recurse through each child GO (if there are any):
		foreach (Transform childT in g.transform)
		{
			//Debug.Log("Searching " + childT.name  + " " );
			FindMissingScriptsRecursivelyInSelectedGameObjectsInGO(childT.gameObject);
		}
	}
	#endregion	
	#region Find Using Script

	private static void FindInSelected()
	{
		GameObject[] go = Selection.gameObjects;
		go_count = 0;
		components_count = 0;
		missing_count = 0;
		foreach (GameObject g in go)
		{
			FindInGO(g);
		}
		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} MonoBehaviour", go_count, components_count, missing_count));
		string json = JsonParser.Serialize(m_ScriptList);

		string savePath = Path.Combine(Application.dataPath, string.Format("UsedScript_{0}.json", DateTime.Now.ToString("yyyy-MM-dd_HH-mm")));
		using (StreamWriter saveFile = File.CreateText(savePath))
		{
			saveFile.Write(json);
			saveFile.Close();
		}

		Debug.Log(json);
	}

	private static void FindInGO(GameObject g)
	{
		go_count++;
		//Component[] components = g.GetComponents<Component>();
		Component[] components = g.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			components_count++;

			Component comp = components[i];

			if (comp != null)
			{
				Type compType = comp.GetType();

				if (string.IsNullOrEmpty(compType.Namespace) == false && compType.Namespace.Contains("UnityEngine"))
					continue;

				missing_count++;
				string s = g.name;
				Transform t = g.transform;
				while (t.parent != null)
				{
					s = t.parent.name + "/" + s;
					t = t.parent;
				}

				if (!m_ScriptList.Contains(compType.Name))
					m_ScriptList.Add(compType.Name);
				Debug.Log(s + " has an " + compType.Name + " script attached in position: " + i, g);
			}
		}
		// Now recurse through each child GO (if there are any):
		foreach (Transform childT in g.transform)
		{
			//Debug.Log("Searching " + childT.name  + " " );
			FindInGO(childT.gameObject);
		}
	}

	#endregion	
	#region Find Prefab Recursively
	private static void FindMissingPrefabsRecursivelyInSelectedGameObjects()
	{
		GameObject[] go = Selection.gameObjects;
		go_count = 0;		
		missing_count = 0;
		foreach (GameObject g in go)
		{
			FindMissingPrefabsRecursivelyInSelectedGameObjectsInGO(g);
		}
		Debug.Log(string.Format("Searched {0} GameObjects, found {1} missing", go_count, missing_count));
	}

	private static void FindMissingPrefabsRecursivelyInSelectedGameObjectsInGO(GameObject g)
	{
		go_count++;
		
		PrefabType prefabType = PrefabUtility.GetPrefabType(g);
		if (prefabType == PrefabType.DisconnectedModelPrefabInstance ||
			prefabType == PrefabType.DisconnectedPrefabInstance ||
			prefabType == PrefabType.MissingPrefabInstance)
		{
			missing_count++;
			Debug.Log(g.name + " was broken prefab in gameobject");
		}

		// Now recurse through each child GO (if there are any):
		foreach (Transform childT in g.transform)
		{
			//Debug.Log("Searching " + childT.name  + " " );
			FindMissingPrefabsRecursivelyInSelectedGameObjectsInGO(childT.gameObject);
		}
	}
	#endregion


	private void MergeUsedScript()
	{
		string[] files = Directory.GetFiles(Application.dataPath, "UsedScript_*.json");

		string savePath = Path.Combine(Application.dataPath, string.Format("UsedScript_{0}.json", DateTime.Now.ToString("yyyy-MM-dd_HH-mm")));

		using (StreamReader sr = new StreamReader(savePath))
		{
			string json = sr.ReadToEnd();
		}
	}

	private void DisconnectSelectedPrefab()
	{
		GameObject[] go = Selection.gameObjects;
		
		foreach (GameObject g in go)
		{
			PrefabUtility.DisconnectPrefabInstance(g);
		}
	}
}

/*
public class FindMissingScript : EditorWindow
{
    [MenuItem("Tools/FindMissingScripts")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FindMissingScript));
    }
 
    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected prefabs"))
        {
            FindInSelected();
        }
    }
    private static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        int go_count = 0, components_count = 0, missing_count = 0;
        foreach (GameObject g in go)
        {
            go_count++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                components_count++;
                if (components[i] == null)
                {
                    missing_count++;
                    string s = g.name;
                    Transform t = g.transform;
                    while (t.parent != null) 
                    {
                        s = t.parent.name +"/"+s;
                        t = t.parent;
                    }
                    Debug.Log (s + " has an empty script attached in position: " + i, g);
                }
            }
        }
 
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }
}

public class FindMissingScriptsRecursively : EditorWindow 
{
    static int go_count = 0, components_count = 0, missing_count = 0;
 
    [MenuItem("Tools/FindMissingScriptsRecursively")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(FindMissingScriptsRecursively));
    }
 
    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in selected GameObjects"))
        {
            FindInSelected();
        }
    }
    private static void FindInSelected()
    {
        GameObject[] go = Selection.gameObjects;
        go_count = 0;
		components_count = 0;
		missing_count = 0;
        foreach (GameObject g in go)
        {
   			FindInGO(g);
        }
        Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} missing", go_count, components_count, missing_count));
    }
 
    private static void FindInGO(GameObject g)
    {
        go_count++;
        Component[] components = g.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            components_count++;
            if (components[i] == null)
            {
                missing_count++;
                string s = g.name;
                Transform t = g.transform;
                while (t.parent != null) 
                {
                    s = t.parent.name +"/"+s;
                    t = t.parent;
                }
                Debug.Log (s + " has an empty script attached in position: " + i, g);
            }
        }
        // Now recurse through each child GO (if there are any):
        foreach (Transform childT in g.transform)
        {
            //Debug.Log("Searching " + childT.name  + " " );
            FindInGO(childT.gameObject);
        }
    }
}

public class FindDfTweenScriptsRecursively : EditorWindow
{
	static int go_count = 0, components_count = 0, missing_count = 0;

	[MenuItem("Tools/FindDfTweenScriptsRecursively")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(FindDfTweenScriptsRecursively));
	}

	public void OnGUI()
	{
		if (GUILayout.Button("Find dfTween Scripts in selected GameObjects"))
		{
			FindInSelected();
		}
	}
	private static void FindInSelected()
	{
		GameObject[] go = Selection.gameObjects;
		go_count = 0;
		components_count = 0;
		missing_count = 0;
		foreach (GameObject g in go)
		{
			FindInGO(g);
		}
		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} dfTweens", go_count, components_count, missing_count));
	}

	private static void FindInGO(GameObject g)
	{
		go_count++;
		Component[] components = g.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			components_count++;
			Component comp = components[i];
			if (comp == null)
				continue;

			if (comp.GetType() == typeof(dfTweenVector2) ||
				comp.GetType() == typeof(dfTweenVector3) ||
				comp.GetType() == typeof(dfTweenColor) ||
				comp.GetType() == typeof(dfTweenColor32) ||
				comp.GetType() == typeof(dfTweenFloat) ||
				comp.GetType() == typeof(dfTweenGroup))
			{
				missing_count++;
				string s = g.name;
				Transform t = g.transform;
				while (t.parent != null)
				{
					s = t.parent.name + "/" + s;
					t = t.parent;
				}
				Debug.Log(s + " has an dfTween script attached in position: " + i, g);
			}
		}
		// Now recurse through each child GO (if there are any):
		foreach (Transform childT in g.transform)
		{
			//Debug.Log("Searching " + childT.name  + " " );
			FindInGO(childT.gameObject);
		}
	}
}

public class FindUsingScriptsRecursively : EditorWindow
{
	static int go_count = 0, components_count = 0, missing_count = 0;

	static List<string> m_ScriptList = new List<string>();

	[MenuItem("Tools/FindUsingScriptsRecursively")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(FindUsingScriptsRecursively));
	}

	public void OnGUI()
	{
		if (GUILayout.Button("Find Using Scripts in selected GameObjects"))
		{
			FindInSelected();
		}
	}
	private static void FindInSelected()
	{
		GameObject[] go = Selection.gameObjects;
		go_count = 0;
		components_count = 0;
		missing_count = 0;
		foreach (GameObject g in go)
		{
			FindInGO(g);
		}
		Debug.Log(string.Format("Searched {0} GameObjects, {1} components, found {2} MonoBehaviour", go_count, components_count, missing_count));
		string json = JsonParser.Serialize(m_ScriptList);

		string savePath = Path.Combine(Application.dataPath, string.Format("UsedScript_{0}.json", DateTime.Now.ToString("yyyy-MM-dd_HH-mm")));
		using (StreamWriter saveFile = File.CreateText(savePath))
		{
			saveFile.Write(json);
			saveFile.Close();
		}

		Debug.Log(json);
	}

	private static void FindInGO(GameObject g)
	{
		go_count++;
		//Component[] components = g.GetComponents<Component>();
		Component[] components = g.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			components_count++;

			Component comp = components[i];

			if (comp != null)
			{
				Type compType = comp.GetType();
				
				if (string.IsNullOrEmpty(compType.Namespace) == false && compType.Namespace.Contains("UnityEngine"))
					continue;

				missing_count++;
				string s = g.name;
				Transform t = g.transform;
				while (t.parent != null)
				{
					s = t.parent.name + "/" + s;
					t = t.parent;
				}

				if (!m_ScriptList.Contains(compType.Name))
					m_ScriptList.Add(compType.Name);
				Debug.Log(s + " has an " + compType.Name + " script attached in position: " + i, g);
			}
		}
		// Now recurse through each child GO (if there are any):
		foreach (Transform childT in g.transform)
		{
			//Debug.Log("Searching " + childT.name  + " " );
			FindInGO(childT.gameObject);
		}
	}
}
*/