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

	[SerializeField]
	private GameObject myCamera;

	[SerializeField]
	private GameObject headModel;

	// Use this for initialization
	void Start () {

		// 自身でなければCopyTransformを削除
		if( photonView != null)
		{
			if( !photonView.isMine  )
			{
				var children = GetComponentsInChildren<CopyTransform>();
				for( int i = children.Length -1 ; i >= 0 ; --i )
				{
					Destroy( children[i] );
				}

			}

			myCamera.SetActive( photonView.isMine );
			headModel.SetActive( !photonView.isMine );
		}

	}
	
	// Update is called once per frame
	void Update () 
	{		
		// とりあえず仮でRを押して頭の回転リセット
		if( Input.GetKeyDown(KeyCode.R) )
		{
			OVRManager.display.RecenterPose();
		}
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
