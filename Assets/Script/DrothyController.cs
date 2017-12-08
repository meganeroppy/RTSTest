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
	}

	public void SetDressColor(int colorIdx)
	{
		skinMesh.materials[ dressMatIdx ].mainTexture = dressTexture[ colorIdx % dressTexture.Length ];
	}
}
