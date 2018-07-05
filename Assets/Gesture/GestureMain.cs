using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureMain : MonoBehaviour {

    public GestureHandler gestureHandler;

    private void Start()
    {
        if (gestureHandler != null)
        {
            gestureHandler.OnTouchBegan += Gesture_OnTouchBegan;
            gestureHandler.OnTouchMoved += Gesture_OnTouchMoved;
            gestureHandler.OnTouchEnded += Gesture_OnTouchEnded;

            gestureHandler.OnTap += Gesture_OnTap;
            gestureHandler.OnSlide += Gesture_OnSlide;
        }
    }
        
    private void Gesture_OnSlide(object sender, SlideEventArgs e)
    {
        Debug.LogFormat("Gesture - OnSlide : {0}", e.SlideType);
    }

    private void Gesture_OnTap(object sender, TouchEventArgs e)
    {
        Debug.LogFormat("Gesture - OnTap");
    }

    private void Gesture_OnTouchEnded(object sender, TouchEventArgs e)
    {
        Debug.LogFormat("Gesture - OnTouchEnded : {0}", e.Delta);
    }

    private void Gesture_OnTouchMoved(object sender, TouchEventArgs e)
    {
        Debug.LogFormat("Gesture - OnTouchMove");
    }

    private void Gesture_OnTouchBegan(object sender, TouchEventArgs e)
    {
        Debug.LogFormat("Gesture - OnTouchBegan");
    }
}
