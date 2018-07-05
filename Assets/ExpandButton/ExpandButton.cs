using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ExpandButton : MonoBehaviour {

	public enum Direction : int
	{
		Horizontal = 0,
		Vertical,
		Circle
	}

	private enum QuadrandPane : int
	{
		First = 0,
		Second,
		Third,
		Fourth
	}

	public CanvasGroup m_ChildContent;
	public Direction m_Direction = Direction.Horizontal;
	public float m_Spacing = 0;
	public Ease m_AnimatonEase = Ease.Linear;
	public float m_Time = 0.1f;

	private RectTransform m_rectTransform;

	public RectTransform rectTransform
	{
		get
		{
			if (m_rectTransform == null)
				m_rectTransform = this.gameObject.GetComponent<RectTransform>();
			return m_rectTransform;
		}
	}
			
	private bool m_IsOn = false;
	private List<Button> m_ChildButtonList;
	private float m_Scale = -1;
	private float m_ChildScale = -1;


	private void Awake()
	{
		if (m_ChildContent != null)
		{
			m_Scale = (m_Direction == Direction.Vertical) ? rectTransform.sizeDelta.y : rectTransform.sizeDelta.x;

			m_ChildButtonList = new List<Button>(m_ChildContent.transform.GetComponentsInChildren<Button>());

			Button btn = m_ChildButtonList[0];
			RectTransform btnRect = (RectTransform)btn.transform;
			m_ChildScale = (m_Direction == Direction.Vertical) ? btnRect.sizeDelta.y : btnRect.sizeDelta.x;
		}
	}

	public void OnClick_OnOffExpandButton()
	{
		m_IsOn = !m_IsOn;

		if (m_IsOn)
		{			
			ExpandAll();
		}
		else
		{
			CollapseAll();
		}
	}

	private void ExpandAll()
	{
		Debug.Log("Expand All Buttons");

		QuadrandPane QuadrantNo = GetQuadrant();

		if (m_Direction == Direction.Horizontal)
		{
			// 1사분면 또는 4사분면 이면, 왼쪽으로 애니메이션
			if (QuadrantNo == QuadrandPane.First || QuadrantNo == QuadrandPane.Fourth)
			{
				float gap = (m_Scale > m_ChildScale) ? (m_Scale - m_ChildScale) / 2 : (m_Scale - m_ChildScale) / 2;

				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];
					
					float pos = (m_ChildScale * (i+1) + (m_Spacing * (i+1))) * -1;
					pos = pos - gap;
					btn.transform.DOLocalMoveX(pos, m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
			else
			{
				float gap = (m_Scale > m_ChildScale) ? (m_Scale - m_ChildScale) / 2 : (m_Scale - m_ChildScale) / 2;

				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float pos = (m_ChildScale * (i + 1) + (m_Spacing * (i + 1)));
					pos = pos + gap;
					btn.transform.DOLocalMoveX(pos, m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
		}
		else if (m_Direction == Direction.Vertical)
		{
			// 1사분면 또는 2사분면 이면, 아래쪽으로 애니메이션
			if (QuadrantNo == QuadrandPane.First || QuadrantNo == QuadrandPane.Second)
			{
				float gap = (m_Scale > m_ChildScale) ? (m_Scale - m_ChildScale) / 2 : (m_Scale - m_ChildScale) / 2;

				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float pos = (m_ChildScale * (i + 1) + (m_Spacing * (i + 1))) * -1;
					pos = pos - gap;
					btn.transform.DOLocalMoveY(pos, m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
			else
			{
				float gap = (m_Scale > m_ChildScale) ? (m_Scale - m_ChildScale) / 2 : (m_Scale - m_ChildScale) / 2;

				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float pos = (m_ChildScale * (i + 1) + (m_Spacing * (i + 1)));
					pos = pos + gap;
					btn.transform.DOLocalMoveY(pos, m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
		}
		else
		{
			float radius = m_Scale + m_Spacing;

			if (QuadrantNo == QuadrandPane.First)
			{
				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float angle = (Mathf.PI / m_ChildButtonList.Count * i) + Mathf.PI;

					btn.transform.DOLocalMove(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius), m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
			else if (QuadrantNo == QuadrandPane.Second)
			{

				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float angle = (Mathf.PI / m_ChildButtonList.Count * i) * -1;

					btn.transform.DOLocalMove(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius), m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
			else if (QuadrantNo == QuadrandPane.Third)
			{
				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float angle = (Mathf.PI / m_ChildButtonList.Count * i);

					btn.transform.DOLocalMove(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius), m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
			else
			{
				for (int i = 0; i < m_ChildButtonList.Count; i++)
				{
					Button btn = m_ChildButtonList[i];

					float angle = ((Mathf.PI / m_ChildButtonList.Count * i) - Mathf.PI) * -1;

                    // using DoTween Asset from AssetStore
					btn.transform.DOLocalMove(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius), m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
				}
			}
		}
	}

	private void CollapseAll()
	{
		Debug.Log("Collapse All Buttons");

		for (int i = 0; i < m_ChildButtonList.Count; i++)
		{
			Button btn = m_ChildButtonList[i];			
			btn.transform.DOLocalMove(Vector3.zero, m_Time).SetDelay(0).SetEase(m_AnimatonEase).SetLoops(1).SetAutoKill(true);
		}
	}

	/// <summary>
	/// 사분면 판단
	/// </summary>	
	private QuadrandPane GetQuadrant()
	{						
		if (rectTransform.localPosition.x > 0 && rectTransform.localPosition.y > 0)
		{
			// 1사분면
			return QuadrandPane.First;
		}
		else if (rectTransform.localPosition.x < 0 && rectTransform.localPosition.y > 0)
		{
			// 2사분면
			return QuadrandPane.Second;
		}
		else if (rectTransform.localPosition.x < 0 && rectTransform.localPosition.y < 0)
		{
			// 3사분면
			return QuadrandPane.Third;
		}
		else
		{
			// 4사분면
			return QuadrandPane.Fourth;
		}
	}
}
