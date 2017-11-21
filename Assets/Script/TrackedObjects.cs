using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedObjects : Photon.MonoBehaviour {

	[SerializeField]
	private CopyTransform head;

	[SerializeField]
	private CopyTransform rightHand;

	[SerializeField]
	private CopyTransform leftHand;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize( GameObject copyTransformHead, GameObject copyTransformRightHand, GameObject copyTransformLeftHand, GameObject offsetObject )
	{
		head.copySource = copyTransformHead;
		head.offsetObject = offsetObject;

		rightHand.copySource = copyTransformRightHand;
		rightHand.offsetObject = offsetObject;

		leftHand.copySource = copyTransformLeftHand;
		leftHand.offsetObject = offsetObject;
	}
}
