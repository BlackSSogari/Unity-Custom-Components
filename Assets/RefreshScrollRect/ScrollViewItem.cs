using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : RefresherItem {

	public Image m_IconImage;

	public override void SetItem()
	{
		base.SetItem();
	}

	public override IEnumerator ItemUpdate()
	{
		Sprite spr = Resources.Load<Sprite>(string.Format("icon{0:D3}", Random.Range(11, 30)));
		if (spr != null)
			m_IconImage.sprite = spr;

		yield return new WaitForSeconds(1);
	}
}
