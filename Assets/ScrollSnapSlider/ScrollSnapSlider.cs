using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ScrollRect))]
public class ScrollSnapSlider : UIBehaviour, IDragHandler, IEndDragHandler
{
	// 스냅 슬라이딩 시작 인덱스
	public int m_StartIndex = 0;
	// 무한 스크롤 여부
	public bool m_WarpAround = false;
	// 스냅 슬라이딩 시간
	public float m_SnapSlidingTimeMS = 200f;

	// 스냅 슬라이딩이 발생하는 퍼센트
	public float m_SnapSlidingTriggerPercent = 5f;
	// 스냅 슬라이딩 발생하는 이동 가속도
	[Range(0f, 10f)]
	public float m_SnapSlidingTriggerAcceleration = 1f;
	
	public class OnLerpCompleteEvent : UnityEvent { }
	public OnLerpCompleteEvent onLerpComplete;

	//public class OnReleaseEvent : UnityEvent<int> { }
	//public OnReleaseEvent onRelease;

	// 현재 동작 셀 인덱스
	private int m_ActualCellIndex;
	// 현재 셀 인덱스
	private int m_CellIndex;

	// 스크롤 기능을 위한 ScrollRect
	private ScrollRect m_ScrollRect;
	// 슬라이딩중 터치를 막기위한 CanvasGroup
	private CanvasGroup m_CanvasGroup;
	// 아이템들이 위치한 Content Object
	private RectTransform m_Content;
	// GridLayout을 적용한 Content의 구성 Cell 사이즈
	private Vector2 m_ContentCellSize;

	// 인덱스 변경(SnapSlide) 트리거 동작 유무
	private bool indexChangeTriggered = false;

	// 원위치로 되돌아가기 유무
	bool isLerping = false;
	// 원위치로 되돌아가기 시작 시간
	DateTime lerpStartedAt;
	// 원위치로 되돌아가기 시작 포지션
	Vector2 releasedPosition;
	// 원위치 포지션
	Vector2 targetPosition;

	#region Unity Call

	protected override void Awake()
	{
		base.Awake();

		onLerpComplete = new OnLerpCompleteEvent();
		//this.onRelease = new OnReleaseEvent();

		m_ActualCellIndex = m_StartIndex;
		m_CellIndex = m_StartIndex;

		m_ScrollRect = GetComponent<ScrollRect>();
		m_CanvasGroup = GetComponent<CanvasGroup>();

		m_Content = m_ScrollRect.content;
		m_ContentCellSize = m_Content.GetComponent<GridLayoutGroup>().cellSize;

		// Content 위치 초기화
		m_Content.anchoredPosition = new Vector2(-m_ContentCellSize.x * m_CellIndex, m_Content.anchoredPosition.y);

		// Item 개수 계산
		int itemCount = GetChildElementCount();

		// Content 최종 사이즈 설정
		SetContentSize(itemCount);

		if (m_StartIndex < itemCount)
		{
			MoveToIndex(m_StartIndex);
		}
	}

	private void LateUpdate()
	{
		if (isLerping)
		{
			LerpToElement();
			if (ShouldStopLerping())
			{
				isLerping = false;
				m_CanvasGroup.blocksRaycasts = true;
				onLerpComplete.Invoke();
				onLerpComplete.RemoveListener(WrapElementAround);
			}
		}
	}

	#endregion
	//===================================================================================
	#region Member

	/// <summary>
	/// Content에 존재하는 Child의 개수를 계산하여 가져온다.
	/// Child를 추가/삭제 하는 기능이 있으므로 매번 계산해야 한다.
	/// </summary>
	private int GetChildElementCount()
	{
		int count = 0;
		foreach (Transform t in m_Content.transform)
		{
			if (t.GetComponent<LayoutElement>() != null)
			{
				count += 1;
			}
		}
		return count;
	}

	/// <summary>
	/// Content의 사이즈를 보유한 Child 아이템의 개수에 따라 세팅한다.
	/// Child를 추가/삭제 하는 기능이 있으므로 매번 계산해야 한다.
	/// </summary>
	private void SetContentSize(int childCount)
	{
		m_Content.sizeDelta = new Vector2(m_ContentCellSize.x * childCount, m_Content.rect.height);
	}

	/// <summary>
	/// ScrollRect의 한 화면에 보여질 Item의 개수를 구하고, 전체 Child개수로 부터 MaxIndex를 계산한다.
	/// </summary>
	private int CalculateMaxIndex()
	{
		int cellPerFrame = Mathf.FloorToInt(m_ScrollRect.GetComponent<RectTransform>().sizeDelta.x / m_ContentCellSize.x);
		return GetChildElementCount() - cellPerFrame;
	}

	/// <summary>
	/// 주어진 Index에 해당하는 좌표를 계산한다.
	/// </summary>	
	private Vector2 CalculateTargetPoisition(int index)
	{
		return new Vector2(-m_ContentCellSize.x * index, m_Content.anchoredPosition.y);
	}

	/// <summary>
	/// 해당 인덱스로 Content 포지션을 강제 이동시킨다.
	/// </summary>
	public void MoveToIndex(int newCellIndex)
	{
		int maxIndex = CalculateMaxIndex();

		if (newCellIndex >= 0 && newCellIndex <= maxIndex)
		{
			m_ActualCellIndex += newCellIndex - m_CellIndex;
			m_CellIndex = newCellIndex;
		}
		// 사용하지 않는 이벤트
		//onRelease.Invoke(cellIndex);

		m_Content.anchoredPosition = CalculateTargetPoisition(m_CellIndex);
	}

	/// <summary>
	/// 해당 인덱스로 Content 포지션을 Snap 이동시킨다.
	/// </summary>
	public void SnapToIndex(int newCellIndex)
	{
		int maxIndex = CalculateMaxIndex();
		if (m_WarpAround && maxIndex > 0)
		{
			m_ActualCellIndex += newCellIndex - m_CellIndex;
			m_CellIndex = newCellIndex;
			onLerpComplete.AddListener(WrapElementAround);
		}
		else
		{
			// when it's the same it means it tried to go out of bounds
			if (newCellIndex >= 0 && newCellIndex <= maxIndex)
			{
				m_ActualCellIndex += newCellIndex - m_CellIndex;
				m_CellIndex = newCellIndex;
			}
		}

		// 사용하지 않는 이벤트
		//onRelease.Invoke(m_CellIndex);

		StartLerping();
	}

	/// <summary>
	/// SnapSliding이 취소되어 원래위치로 되돌아가도록 세팅시작
	/// </summary>
	private void StartLerping()
	{
		releasedPosition = m_Content.anchoredPosition;
		targetPosition = CalculateTargetPoisition(m_CellIndex);
		lerpStartedAt = DateTime.Now;
		m_CanvasGroup.blocksRaycasts = false;
		isLerping = true;
	}

	/// <summary>
	/// 원래위치 근처에 도달하면 되돌아기 정지
	/// </summary>
	private bool ShouldStopLerping()
	{
		return Mathf.Abs(m_Content.anchoredPosition.x - targetPosition.x) < 0.001;
	}

	/// <summary>
	/// Drag를 통해서 SnapSliding이 실행되어야 하는지 판단.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	private bool IndexShouldChangeFromDrag(PointerEventData data)
	{
		// acceleration was above threshold
		if (indexChangeTriggered)
		{
			indexChangeTriggered = false;
			return true;
		}
		// dragged beyond trigger threshold
		//var offset = m_ScrollRect.content.anchoredPosition.x + m_CellIndex * m_ContentCellSize.x;
		var offset = m_Content.anchoredPosition.x + m_CellIndex * m_ContentCellSize.x;
		var normalizedOffset = Mathf.Abs(offset / m_ContentCellSize.x);

		return normalizedOffset * 100f > m_SnapSlidingTriggerPercent;
	}

	/// <summary>
	/// SnapSliding이 취소되어 원래위치로 되돌아간다.
	/// </summary>
	void LerpToElement()
	{
		float t = (float)((DateTime.Now - lerpStartedAt).TotalMilliseconds / m_SnapSlidingTimeMS);
		float newX = Mathf.Lerp(releasedPosition.x, targetPosition.x, t);
		m_Content.anchoredPosition = new Vector2(newX, m_Content.anchoredPosition.y);
	}

	/// <summary>
	/// 무한 스크롤 작동
	/// </summary>
	void WrapElementAround()
	{
		if (m_CellIndex <= 0)
		{
			var elements = m_Content.GetComponentsInChildren<LayoutElement>();
			elements[elements.Length - 1].transform.SetAsFirstSibling();
			m_CellIndex += 1;
			m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x - m_ContentCellSize.x, m_Content.anchoredPosition.y);
		}
		else if (m_CellIndex >= (CalculateMaxIndex() - 1))
		{
			var element = m_Content.GetComponentInChildren<LayoutElement>();
			element.transform.SetAsLastSibling();
			m_CellIndex -= 1;
			m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x + m_ContentCellSize.x, m_Content.anchoredPosition.y);
		}
	}

	#endregion
	//===================================================================================
	#region Drag Handler

	public void OnDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		float dx = eventData.delta.x;
		float dt = Time.deltaTime * 1000f;
		float acceleration = Mathf.Abs(dx / dt);
		if (acceleration > m_SnapSlidingTriggerAcceleration && acceleration != Mathf.Infinity)
		{
			indexChangeTriggered = true;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		if (IndexShouldChangeFromDrag(eventData))
		{
			int direction = (eventData.pressPosition.x - eventData.position.x) > 0f ? 1 : -1;
			SnapToIndex(m_CellIndex + direction);
		}
		else
		{
			StartLerping();
		}
	}

	#endregion
	//===================================================================================
	#region Unused Methods

	public void AddChildItem(LayoutElement element)
	{
		element.transform.SetParent(m_Content.transform, false);
		SetContentSize(GetChildElementCount());
	}

	public void RemoveChildItem()
	{
		LayoutElement[] elements = m_Content.GetComponentsInChildren<LayoutElement>();
		Destroy(elements[elements.Length - 1].gameObject);
		SetContentSize(GetChildElementCount() - 1);
		if (m_CellIndex == CalculateMaxIndex())
		{
			m_CellIndex -= 1;
		}
	}

	public void UnshiftLayoutElement(LayoutElement element)
	{
		m_CellIndex += 1;
		element.transform.SetParent(m_Content.transform, false);
		element.transform.SetAsFirstSibling();
		SetContentSize(GetChildElementCount());
		m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x - m_ContentCellSize.x, m_Content.anchoredPosition.y);
	}

	public void ShiftLayoutElement()
	{
		Destroy(GetComponentInChildren<LayoutElement>().gameObject);
		SetContentSize(GetChildElementCount() - 1);
		m_CellIndex -= 1;
		m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x + m_ContentCellSize.x, m_Content.anchoredPosition.y);
	}

	public int GetCurrentIndex()
	{
		int count = GetChildElementCount();
		int mod = m_ActualCellIndex % count;
		return mod >= 0 ? mod : count + mod;
	}

	public void SnapToNext()
	{
		if (IsActive())
			SnapToIndex(m_CellIndex + 1);
	}

	public void SnapToPrev()
	{
		if (IsActive())
			SnapToIndex(m_CellIndex - 1);
	}

	#endregion
}
