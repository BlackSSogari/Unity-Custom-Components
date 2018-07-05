using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Swipe(Slide) 방향 타입
/// </summary>
public enum SType
{
    LEFT,
    RIGHT,
    UP,
    DOWN
}

/// <summary>
/// 터치 이벤트 Args
/// </summary>
public class TouchEventArgs : EventArgs
{
    public Vector2 Start { get; private set; }
    public Vector2 End { get; private set; }
    public Vector2 Delta { get; private set; }
    
    public TouchEventArgs(Vector2 began, Vector2 moved)
    {
        this.Start = new Vector2(began.x, began.y);
        this.End = new Vector2(moved.x, moved.y);
        this.Delta = End - Start;        
    }
}

/// <summary>
/// Slide 이벤트 Args
/// </summary>
public class SlideEventArgs : TouchEventArgs
{
    public SType SlideType { get; private set; }

    public SlideEventArgs(Vector2 began, Vector2 moved)
        : base(began, moved)
    {
        if (began.x - moved.x >= 25)
        {
            this.SlideType = SType.LEFT;
        }

        if (moved.x - began.x >= 25)
        {
            this.SlideType = SType.RIGHT;
        }

        if (began.y - moved.y >= 25)
        {
            this.SlideType = SType.DOWN;
        }

        if (moved.y - began.y >= 25)
        {
            this.SlideType = SType.UP;
        }
    }
}

public class GestureHandler : MonoBehaviour {

    #region Detect Gesture Touch

    public bool EnableInput = true;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private bool m_IsMouseDown;
#endif
    private Vector2[] m_TouchBeganPoints = new Vector2[2];

    public event EventHandler<TouchEventArgs> OnTouchBegan;
    public event EventHandler<TouchEventArgs> OnTouchMoved;
    public event EventHandler<TouchEventArgs> OnTouchEnded;
    public event EventHandler<TouchEventArgs> OnTap;
    public event EventHandler<SlideEventArgs> OnSlide;

    void Update()
    {
        if (!EnableInput) return;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (!m_IsMouseDown && Input.GetMouseButton(0))
        {
            m_IsMouseDown = true;
            Vector2 mp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            m_TouchBeganPoints[0] = mp;

            if (OnTouchBegan != null)
            {
                OnTouchBegan(this, new TouchEventArgs(Input.mousePosition, mp));
            }
        }
        else if (m_IsMouseDown)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 mp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

                if (OnTouchMoved != null)
                {
                    OnTouchMoved(this, new TouchEventArgs(m_TouchBeganPoints[0], mp));
                }
            }
            else
            {
                Vector2 mp = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                m_IsMouseDown = false;

                TouchEventArgs eventArgs = new TouchEventArgs(m_TouchBeganPoints[0], mp);
                if (OnTouchEnded != null)
                {
                    OnTouchEnded(this, eventArgs);
                }

                if ((m_TouchBeganPoints[0] - mp).magnitude < 25)
                {
                    if (OnTap != null)
                    {
                        OnTap(this, eventArgs);
                    }
                }
                else
                {
                    if (OnSlide != null)
                    {
                        OnSlide(this, new SlideEventArgs(m_TouchBeganPoints[0], mp));
                    }
                }
            }
        }

#else
        int count = Input.touchCount;
        if (count == 0) return;

        for (int i = 0; i < count; ++i)
        {
            Touch touch = Input.GetTouch(i);
            int id = touch.fingerId;

            if (count == 1 && id == 0)
            {
				if (touch.phase == TouchPhase.Began)
				{
					m_TouchBeganPoints[0] = touch.position;
					if (OnTouchBegan != null)
					{
						OnTouchBegan(this, new TouchEventArgs(touch.position, touch.position));
					}					
				}
				else if (touch.phase == TouchPhase.Moved)
				{
					if (OnTouchMoved != null)
					{
						OnTouchMoved(this, new TouchEventArgs(m_TouchBeganPoints[0], touch.position));
					}
				}
				else if (touch.phase == TouchPhase.Ended)
				{
					TouchEventArgs eventArgs = new TouchEventArgs(m_TouchBeganPoints[0], touch.position);					
					if (OnTouchEnded != null)
					{
						OnTouchEnded(this, eventArgs);
					}

					if ((m_TouchBeganPoints[0] - touch.position).magnitude < 25)
					{
						if (OnTap != null)
						{
							OnTap(this, eventArgs);
						}
					}
					else
					{
						SlideEventArgs slideEventArgs = new SlideEventArgs(m_TouchBeganPoints[0], touch.position);						
						if (OnSlide != null)
						{
							OnSlide(this, slideEventArgs);
						}
					}
				}
			}
        }
#endif  
    }

    #endregion
}
