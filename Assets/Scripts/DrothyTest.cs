using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrothyTest : MonoBehaviour
{
	public  float speed = 5f;
	public List<Animator> anim;
	 	
	public Transform drothyBase;

	enum Anim
	{
		Stand,
		MouseOpen,
		MouseSmile,
		EyeClose,
		Go2,
		Furi2,
		OpenArm,
		Count,
	}
	int animIdx = 0;

	int colorIdx = 0;

	[SerializeField]
	private Texture[] colorTexs;

	[SerializeField]
	private SkinnedMeshRenderer dressMesh;

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

		if( Input.GetKeyDown( KeyCode.C ) )
		{
			ChangeColor();
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
		anim.ForEach( a => a.SetTrigger( newAnim.ToString() ) );
		Debug.Log( newAnim.ToString() + " is triggered " );
	}

	void ChangeColor()
	{
		colorIdx++;

		if( colorIdx >= colorTexs.Length )
		{
			colorIdx = 0;
		}	

		// ドレスのマテリアル差し替え
		dressMesh.materials[1].mainTexture = colorTexs[colorIdx];
	}

}
