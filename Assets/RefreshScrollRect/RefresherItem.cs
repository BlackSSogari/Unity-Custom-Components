using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefresherItem : MonoBehaviour {

	virtual public void SetItem() { }

	virtual public IEnumerator ItemUpdate() { yield return null; }
}
