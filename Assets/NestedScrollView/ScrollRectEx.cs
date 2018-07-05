using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// https://bitbucket.org/ddreaper/unity-ui-extensions/wiki/Controls/ScrollRectEx

public class ScrollRectEx : ScrollRect
{
	private CanvasGroup m_CanvasGroup;
	private ScrollRectRefresher m_Refresher;
	private bool isContentLock = false;
	private float thresholdHoldContent = 100f;
	private float minThreshold = 100f;

	private bool routeToParent = false;

	protected override void Awake()
	{
		base.Awake();

		if (vertical && !horizontal && movementType == MovementType.Elastic)
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
			m_Refresher = GetComponentInChildren<ScrollRectRefresher>(true);

			thresholdHoldContent = ((RectTransform)this.transform).sizeDelta.y * 0.3f;
			if (thresholdHoldContent < minThreshold)
				thresholdHoldContent = minThreshold;
		}
	}

	protected override void LateUpdate()
	{
		if (vertical && !horizontal && movementType == MovementType.Elastic && isContentLock)
		{
			return;
		}

		base.LateUpdate();
	}

	/// <summary>
	/// Do action for all parents
	/// </summary>
	private void DoForParents<T>(Action<T> action) where T : IEventSystemHandler
	{
		Transform parent = transform.parent;
		while (parent != null)
		{
			foreach (var component in parent.GetComponents<Component>())
			{
				if (component is T)
					action((T)(IEventSystemHandler)component);
			}
			parent = parent.parent;
		}
	}

	/// <summary>
	/// Always route initialize potential drag event to parents
	/// </summary>
	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		DoForParents<IInitializePotentialDragHandler>((parent) =>
		{
			parent.OnInitializePotentialDrag(eventData);
		});
		base.OnInitializePotentialDrag(eventData);
	}

	/// <summary>
	/// Drag event
	/// </summary>
	public override void OnDrag(PointerEventData eventData)
	{
		if (routeToParent)
		{
			DoForParents<IDragHandler>((parent) =>
			{
				parent.OnDrag(eventData);
			});
		}
		else
		{
			base.OnDrag(eventData);
		}

		if (!isContentLock && vertical && !horizontal && movementType == MovementType.Elastic && content.anchoredPosition.y <= (thresholdHoldContent * -1))
		{
			LockHoldContent();
			eventData.dragging = false;
			eventData.pointerDrag = null;						
			OnEndDrag(eventData);
		}
	}

	/// <summary>
	/// Begin drag event
	/// </summary>
	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
		{
			routeToParent = true;
		}
		else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta. y))
		{
			routeToParent = true;
		}
		else
		{
			routeToParent = false;
		}

		if (routeToParent)
		{
			DoForParents<IBeginDragHandler>((parent) =>
			{
				parent.OnBeginDrag(eventData);
			});
		}
		else
		{
			base.OnBeginDrag(eventData);
		}
	}

	/// <summary>
	/// End drag event
	/// </summary>
	public override void OnEndDrag(PointerEventData eventData)
	{		
		if (routeToParent)
		{
			DoForParents<IEndDragHandler>((parent) =>
			{
				parent.OnEndDrag(eventData);
			});
		}
		else
		{
			base.OnEndDrag(eventData);
		}

		routeToParent = false;
	}

	public override void OnScroll(PointerEventData eventData)
	{
		if (!horizontal && Math.Abs(eventData.scrollDelta.x) > Math.Abs(eventData.scrollDelta.y))
		{
			routeToParent = true;
		}
		else if (!vertical && Math.Abs(eventData.scrollDelta.x) < Math.Abs(eventData.scrollDelta.y))
		{
			routeToParent = true;
		}
		else
			routeToParent = false;

		if (routeToParent)
			DoForParents<IScrollHandler>((parent) => {
				parent.OnScroll(eventData);
			});
		else
			base.OnScroll(eventData);
	}

	public void LockHoldContent()
	{
		if (m_Refresher == null || m_CanvasGroup == null)
			return;

		isContentLock = true;
		m_CanvasGroup.blocksRaycasts = false;
				
		m_Refresher.StartRefresh();
				
		StartCoroutine(CoUpdateItems(() =>
		{
			m_Refresher.StopRefresh();
			UnlockHoldContent();
		}));
	}

	private IEnumerator CoUpdateItems(Action onComplete)
	{
		var items = content.GetComponentsInChildren<RefresherItem>(true);
		for (int i = 0; i < items.Length; i++)
		{
			yield return StartCoroutine(items[i].ItemUpdate());
		}

		if (onComplete != null)
			onComplete();
	}

	public void UnlockHoldContent()
	{
		if (m_Refresher == null || m_CanvasGroup == null)
			return;

		isContentLock = false;
		m_CanvasGroup.blocksRaycasts = true;
	}
}
