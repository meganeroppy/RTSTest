using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
	public float range = 0.2f;
	public float speed = 0.2f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.localPosition = new Vector3( transform.localPosition.x, Mathf.Sin( Time.frameCount * speed ) * range, transform.localPosition.z);
	}
}
