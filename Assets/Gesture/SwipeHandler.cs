using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Swipe 이벤트 Args
/// </summary>
public class SwipeEventArgs : TouchEventArgs
{
    public SType SwipeType { get; private set; }

    public SwipeEventArgs(Vector2 began, Vector2 moved, SType type)
        : base(began, moved)
    {
        this.SwipeType = type;
    }
}

public class SwipeHandler : MonoBehaviour
{
	#region Detect Swipe Gesture

	// https://pfonseca.com/swipe-detection-on-unity

	private float fingerStartTime = 0.0f;
	private Vector2 fingerStartPos = Vector2.zero;

	private bool isSwipe = false;
	private float minSwipeDist = 50.0f;
	private float maxSwipeTime = 0.5f;

    public event EventHandler<SwipeEventArgs> OnDetectSwipe;
    
    private void DetectSwipeGesture()
	{
		if (Input.touchCount > 0)
		{
			foreach (Touch touch in Input.touches)
			{
				switch (touch.phase)
				{
					case TouchPhase.Began:
						/* this is a new touch */
						isSwipe = true;
						fingerStartTime = Time.time;
						fingerStartPos = touch.position;
						break;

					case TouchPhase.Canceled:
						/* The touch is being canceled */
						isSwipe = false;
						break;

					case TouchPhase.Ended:

						float gestureTime = Time.time - fingerStartTime;
						float gestureDist = (touch.position - fingerStartPos).magnitude;

						if (isSwipe && gestureTime < maxSwipeTime && gestureDist > minSwipeDist)
						{
							Vector2 direction = touch.position - fingerStartPos;
							Vector2 swipeType = Vector2.zero;

							if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
							{
								// the swipe is horizontal:
								swipeType = Vector2.right * Mathf.Sign(direction.x);
							}
							else
							{
								// the swipe is vertical:
								swipeType = Vector2.up * Mathf.Sign(direction.y);
							}

							if (swipeType.x != 0.0f)
							{
								if (swipeType.x > 0.0f)
								{
									// MOVE RIGHT									
                                    if (OnDetectSwipe != null)
                                        OnDetectSwipe(this, new SwipeEventArgs(fingerStartPos, touch.position, SType.RIGHT));
								}
								else
								{
									// MOVE LEFT									
                                    if (OnDetectSwipe != null)
                                        OnDetectSwipe(this, new SwipeEventArgs(fingerStartPos, touch.position, SType.LEFT));
                                }
							}

							if (swipeType.y != 0.0f)
							{
								if (swipeType.y > 0.0f)
								{
									// MOVE UP									
                                    if (OnDetectSwipe != null)
                                        OnDetectSwipe(this, new SwipeEventArgs(fingerStartPos, touch.position, SType.UP));
                                }
								else
								{
									// MOVE DOWN									
                                    if (OnDetectSwipe != null)
                                        OnDetectSwipe(this, new SwipeEventArgs(fingerStartPos, touch.position, SType.DOWN));
                                }
							}

						}

						break;
				}
			}
		}
	}

    #endregion

    private void Update()
    {
        DetectSwipeGesture();
    }
}
