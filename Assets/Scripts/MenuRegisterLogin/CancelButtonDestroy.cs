using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelButtonDestroy : MonoBehaviour {

	public void CancelOnClick()
    {
        Destroy(gameObject);
    }
}
