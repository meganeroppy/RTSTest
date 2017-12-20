using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrothyController : MonoBehaviour 
{
	private Animator anim;
	private bool walking;

	[SerializeField]
	private float threasholdMoveSpeed = 1f;

	[SerializeField]
	private Transform modelRoot;

	private Vector3 prevPosition;

	[SerializeField]
	private SkinnedMeshRenderer skinMesh;

	private int dressMatIdx = 1;

	[SerializeField]
	private Texture[] dressTexture;

	// Use this for initialization
	void Start ()
	{
		anim = GetComponent<Animator>();
		prevPosition = modelRoot.localPosition;
		walking = false;
	}

	// Update is called once per frame
	void Update () 
	{
		var vDiff = (prevPosition - modelRoot.localPosition);
//		Debug.Log( vDiff );

		var diff = vDiff.magnitude;

//			Debug.Log( diff.ToString() );

		walking = diff >= threasholdMoveSpeed;

		if( walking != anim.GetBool("Walking") )
		{
			anim.SetBool("Walking", walking);
		}
		
		prevPosition = modelRoot.localPosition;

		if( falling )
		{			
			if( enableLoop && Mathf.Abs( transform.position.y ) > loopThresholdHeight )
			{
				transform.position = new Vector3( transform.position.x, originHeight, transform.position.z);
			}

			var speedDif = fallingSpeedMax - fallingSpeedMin;
			if( speedDif == 0 ) speedDif = 1;

			var speed = fallingSpeedMin + Mathf.PingPong(Time.time * interval, speedDif);
			transform.position += Vector3.down * speed * Time.deltaTime;

		}
	}

	public void SetDressColor(int colorIdx)
	{
		skinMesh.materials[ dressMatIdx ].mainTexture = dressTexture[ colorIdx % dressTexture.Length ];
	}

	private bool falling = false;
	public float fallingSpeedMax = 1f;
	public float fallingSpeedMin = 1f;
	public float interval = 1f;
	public bool enableLoop = false;
	public void SetIsFalling()
	{
		SetIsFalling(!falling);
	}
	public void SetIsFalling(bool value)
	{
		falling = value;
	}

	private float originHeight = 0;
	public float loopThresholdHeight = 20f;
	public void StartFalling()
	{
		originHeight = transform.position.y;
		SetIsFalling( true );
	}
}
