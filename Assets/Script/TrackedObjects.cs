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

	[SerializeField]
	private GameObject cameraImage;

	/// <summary>
	/// 観測者か？
	/// </summary>
	[PunRPC]
	private void SetObserver( bool value )
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		_isObserver = value;
		var children = GetComponentsInChildren<CopyTransform>();
		foreach( CopyTransform c in children )
		{
			c.enabled = !_isObserver;
		}

		rightHand.gameObject.SetActive( !_isObserver );
		leftHand.gameObject.SetActive( !_isObserver );
		headModel.SetActive( !_isObserver );

		cameraImage.SetActive( _isObserver );

		if( _isObserver )
		{
			head.transform.localPosition = Vector3.zero;
		}
	}		
	private bool _isObserver;

	[SerializeField]
	private ObserverController observerController;

	// Use this for initialization
	void Start () {

		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;
		// 自身でなければCopyTransformを削除
		if( photonView != null)
		{
			if( !photonView.isMine  )
			{
				var children = GetComponentsInChildren<CopyTransform>();
				foreach( CopyTransform c in children )
				{
					c.enabled = false;
				}

			}

			myCamera.SetActive( photonView.isMine );
			headModel.SetActive( !photonView.isMine );

			observerController.enabled = false;
		}

	}
	
	// Update is called once per frame
	void Update () 
	{		
		if( photonView != null && !photonView.isMine  ) return;

		// とりあえず仮でRを押して頭の回転リセット
		if( Input.GetKeyDown(KeyCode.R) )
		{
			OVRManager.display.RecenterPose();
		}

		// キーボードでOを押すと観測者になる
		if( Input.GetKeyDown(KeyCode.O) )
		{
			photonView.RPC( "SetObserver", PhotonTargets.AllBuffered, !_isObserver );
			observerController.enabled = _isObserver;
		}
	}

	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize( GameObject copyTransformHead, GameObject copyTransformRightHand, GameObject copyTransformLeftHand, GameObject offsetObject )
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		head.copySource = copyTransformHead;
		head.offsetObject = offsetObject;

		rightHand.copySource = copyTransformRightHand;
		rightHand.offsetObject = offsetObject;

		leftHand.copySource = copyTransformLeftHand;
		leftHand.offsetObject = offsetObject;
	}
}
