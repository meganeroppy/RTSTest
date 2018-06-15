using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVmove : MonoBehaviour {

	public Transform jointA;
	void Start() {
		GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", Vector2.zero);
	}

	void Update() {
		//var x = Mathf.Repeat(Time.time * scrollSpeedX, 1);
		//var y = Mathf.Repeat(Time.time * scrollSpeedY, 1);
		var x = jointA.localRotation.y;
		var y = jointA.localRotation.z;

		var offset = new Vector2(x, y);

		GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
	}
}
