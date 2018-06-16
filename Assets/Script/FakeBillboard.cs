using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeBillboard : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		var dir = Camera.main.transform.forward;
		transform.rotation = Quaternion.Euler( dir );
	}
}
