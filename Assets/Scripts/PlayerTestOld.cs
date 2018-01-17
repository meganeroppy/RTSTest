using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTestOld : MonoBehaviour 
{
	public float moveSpeed = 1f;
	public float rotSpeed = 50f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		var h = Input.GetAxis("Horizontal");
		var v = Input.GetAxis("Vertical");
		var up = Input.GetKey(KeyCode.Space) ? 1f : Input.GetKey( KeyCode.LeftShift ) ? -1f : 0;

		transform.localPosition += transform.forward * v * moveSpeed * Time.deltaTime;
		transform.localPosition += transform.right * h * moveSpeed * Time.deltaTime;
		transform.localPosition += transform.up * up * moveSpeed * Time.deltaTime;

		var rot = Input.GetKey(KeyCode.U) ? -1f : Input.GetKey(KeyCode.I) ? 1f : 0;

		transform.Rotate( 0, rot * rotSpeed * Time.deltaTime , 0 );
	}
}
