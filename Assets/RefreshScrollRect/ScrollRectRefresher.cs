using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Required DOTween Asset
public class ScrollRectRefresher : MonoBehaviour
{
	public CanvasGroup m_RefreshIndicator;
			
	private void Awake()
	{
		m_RefreshIndicator = GetComponent<CanvasGroup>();
		m_RefreshIndicator.alpha = 0f;
	}

	public void StartRefresh()
	{
		if (m_RefreshIndicator != null)
		{
			m_RefreshIndicator.alpha = 0f;
			m_RefreshIndicator.gameObject.SetActive(true);
			m_RefreshIndicator.DOFade(1f, 0.3f).SetEase(Ease.OutExpo).SetDelay(0f).SetLoops(1).SetAutoKill(true);
			m_RefreshIndicator.transform.DOLocalRotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).
					SetDelay(0.1f).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).
					SetAutoKill(true).SetId("RefresherRotateTween");
		}
	}

	public void StopRefresh()
	{
		if (m_RefreshIndicator != null)
		{
			m_RefreshIndicator.alpha = 1f;
			DOTween.Kill("RefresherRotateTween");
			m_RefreshIndicator.DOFade(0f, 0.3f).SetEase(Ease.OutExpo).SetDelay(0f).SetLoops(1).SetAutoKill(true).OnComplete(() =>
			{
				m_RefreshIndicator.alpha = 0f;
				m_RefreshIndicator.gameObject.SetActive(false);
			});
		}
	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(ScrollRectRefresher))]
public class ScrollRectRefresherEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Start"))
		{
			ScrollRectRefresher rf = target as ScrollRectRefresher;
			rf.StartRefresh();
		}
		if (GUILayout.Button("Stop"))
		{
			ScrollRectRefresher rf = target as ScrollRectRefresher;
			rf.StopRefresh();
		}
	}
}

#endif