using System;

// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle

public class FisherYatesShuffle
{				
	/// <summary>
	/// An improved version (Durstenfeld) of the Fisher-Yates algorithm with O(n) time complexity
	/// Permutes the given array
	/// </summary>
	/// <param name="array">array to be shuffled</param>
	public static void Shuffle<T>(T[] array)
	{
		Random r = new Random();
		for (int i = array.Length - 1; i > 0; i--)
		{
			int index = r.Next(i);
			//swap
			T tmp = array[index];
			array[index] = array[i];
			array[i] = tmp;
		}
	}

	public static T[] ShuffleArray<T>(T[] array)
	{
		Random r = new Random();
		for (int i = array.Length - 1; i > 0; i--)
		{
			int index = r.Next(i);
			//swap
			T tmp = array[index];
			array[index] = array[i];
			array[i] = tmp;
		}

		return array;
	}

	// Seed를 설정해주면 일정하게 랜덤값이 출력된다.
	public static T[] ShuffleArray<T>(T[] array, int _seed)
	{
		Random r = new Random(_seed);
		for (int i = array.Length - 1; i > 0; i--)
		{
			int index = r.Next(i);
			//swap
			T tmp = array[index];
			array[index] = array[i];
			array[i] = tmp;
		}

		return array;
	}
}
