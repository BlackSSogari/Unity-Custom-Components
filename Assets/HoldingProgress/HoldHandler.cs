using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {

	public GameObject m_Prefab;
	public float m_WaitingTime = 1f;

	private float m_StartTime;

    public event EventHandler OnHoldDone;

	private Slider m_PrefabSlider;

	public void OnPointerDown(PointerEventData e)
	{
		m_StartTime = Time.realtimeSinceStartup;

		GameObject sliderObj = GameObject.Instantiate(m_Prefab, this.transform);
		if (sliderObj != null)
		{
			m_PrefabSlider = sliderObj.GetComponent<Slider>();
			RectTransform rectTrans = (RectTransform)m_PrefabSlider.transform;

			Vector2 localPos;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTrans, e.position, Camera.main, out localPos))
			{
				rectTrans.localPosition = localPos;
			}

			StartCoroutine(CoWaitingDrag());
		}
	}

	private IEnumerator CoWaitingDrag()
	{		
		while ((Time.realtimeSinceStartup - m_StartTime) < m_WaitingTime)
		{
			m_PrefabSlider.value = (Time.realtimeSinceStartup - m_StartTime) / m_WaitingTime;

			yield return null;
		}

		m_PrefabSlider.value = 1f;
        if (OnHoldDone != null)
            OnHoldDone(this, null);
	}

	public void OnPointerUp(PointerEventData e)
	{
		GameObject.Destroy(m_PrefabSlider.gameObject);
	}
}
