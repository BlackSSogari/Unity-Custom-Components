using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleSample : MonoBehaviour {

    public GameObject[] ShuffleArray;

    private bool isStart = false;

    private void Start()
    {
        InvokeRepeating("ExcuteShuffle", 3f, 3f);
    }

    private void ExcuteShuffle()
    {
        float startTime = Time.realtimeSinceStartup;
        FisherYatesShuffle.Shuffle<GameObject>(ShuffleArray);
        double elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.LogFormat("Shuffle done! : {0}sec", elapsedTime);

        isStart = true;
    }

    private void Update()
    {
        if (isStart)
        {
            for (int i = 0; i < ShuffleArray.Length; i++)
            {
                GameObject obj = ShuffleArray[i];
                obj.transform.localPosition = new Vector3(Mathf.Lerp(obj.transform.position.x, (-7.5f + (2.5f * i)), Time.deltaTime * 4f), 0, 0);
            }
        }
    }
}
