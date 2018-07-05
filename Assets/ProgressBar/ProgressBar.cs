using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ProgressBar : MonoBehaviour {

	public Slider m_ProgressSlider;
	public Text m_ProgressText;

	private string m_ProgressString = "";
	public string ProgressString 
	{
		get { return m_ProgressString; }
		set { m_ProgressString = value; }
	}

	// Use this for initialization
	void Start () 
	{
		if (m_ProgressSlider != null)
		{
			m_ProgressSlider.minValue = 0;
			m_ProgressSlider.maxValue = 1;
			m_ProgressSlider.value = 0f;
		}		
	}

	/// <summary>
	/// 0 - 1 사이의 값으로 프로그레스바를 세팅한다.
	/// </summary>
	public void SetProgressValue(float v)
	{
		if (m_ProgressSlider != null)
			m_ProgressSlider.value = v;
	}

	public void OnValueChanged(Single e)
	{		
		if (m_ProgressText != null)
		{			
			m_ProgressText.text = string.Format("{0}{1}", ProgressString, e.ToString("P"));
		}
	}
}
