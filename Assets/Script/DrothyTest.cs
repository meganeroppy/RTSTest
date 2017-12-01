using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrothyTest : MonoBehaviour
{
	public  float speed = 5f;
	public Animator anim;
	 	
	public Transform drothyBase;

	enum Anim
	{
		Stand,
		MouseOpen,
		MouseSmile,
		EyeClose,
		Go2,
		Furi2,
		Count,
	}

	int animIdx = 0;

	// Update is called once per frame
	void Update () 
	{
		UpdatePosition();
		UpdateInput();
	}

	void UpdatePosition()
	{
		var h = Input.GetAxis("Horizontal");
		var v = Input.GetAxis("Vertical");

		drothyBase.position += new Vector3( h, 0, v ) * speed * Time.deltaTime;	}

	void UpdateInput()
	{
		if( Input.GetKeyDown( KeyCode.P ) )
		{
			ExecAnim();
		}
	}
	void ExecAnim()
	{
		animIdx++;

		if( animIdx >= (int)Anim.Count )
		{
			animIdx = 0;
		}

		var newAnim = (Anim)animIdx;
		anim.SetTrigger( newAnim.ToString() );
		Debug.Log( newAnim.ToString() + " is triggered " );
	}

	void ChangeColor(){}
}
