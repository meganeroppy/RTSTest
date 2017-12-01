using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrothyTest : MonoBehaviour
{
	public  float speed = 5f;
	public Animator anim;
	 	
	public Transform drothyBase;

	// Update is called once per frame
	void Update () 
	{
		var h = Input.GetAxis("Horizontal");
		var v = Input.GetAxis("Vertical");

		drothyBase.position += new Vector3( h, 0, v ) * speed * Time.deltaTime;
	}

	void ExecAnim(){}

	void ChangeColor(){}
}
