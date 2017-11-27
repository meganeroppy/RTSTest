using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackedObjects : Photon.MonoBehaviour {

	[SerializeField]
	private CopyTransform head;

	[SerializeField]
	private CopyTransform rightHand;

	[SerializeField]
	private CopyTransform leftHand;

	[SerializeField]
	private Camera myCamera;

	[SerializeField]
	private AudioListener audioListener;

	[SerializeField]
	private MeshRenderer headModel;

	[SerializeField]
	private GameObject cameraImage;

	[SerializeField]
	private Text playerLabel;

	[SerializeField]
	private ObserverController observerController;

	// Use this for initialization
	void Start () 
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		// 自身でなければCopyTransformを削除
		if( !photonView.isMine  )
		{
			var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}
		}

		myCamera.enabled = photonView.ownerId == 0 || photonView.isMine;
		audioListener.enabled = photonView.ownerId == 0 || photonView.isMine; 
		headModel.enabled = !photonView.isMine && !_isObserver;
		playerLabel.enabled = !photonView.isMine && !_isObserver;
	}
	
	// Update is called once per frame
	void Update () 
	{		
		if( photonView.ownerId != 0 && !photonView.isMine  ) return;

		// とりあえず仮でRを押して頭の回転リセット
		if( Input.GetKeyDown(KeyCode.R) )
		{
			OVRManager.display.RecenterPose();
		}

		// キーボードでOを押すと観測者になる
		if( Input.GetKeyDown(KeyCode.O) )
		{
			photonView.RPC( "SetObserver", PhotonTargets.AllBuffered, !_isObserver );
		}
	}
		
	/// <summary>
	/// 初期化
	/// </summary>
	public void Initialize( GameObject copyTransformHead, GameObject copyTransformRightHand, GameObject copyTransformLeftHand, GameObject offsetObject, int playerId )
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		if( playerId == 0 || photonView.ownerId == 0)
		{
			head.enabled = false;
			rightHand.enabled = false;
			leftHand.enabled = false;
			if( photonView.ownerId != 0 && photonView.isMine )
			{
				photonView.RPC( "SetObserver", PhotonTargets.AllBuffered, !_isObserver );
			}
			else
			{
				SetObserver( true );
			}
			return;
		}

		head.copySource = copyTransformHead;
		head.offsetObject = offsetObject;

		rightHand.copySource = copyTransformRightHand;
		rightHand.offsetObject = offsetObject;

		leftHand.copySource = copyTransformLeftHand;
		leftHand.offsetObject = offsetObject;

		playerLabel.text = "PLAYER[ " + playerId.ToString() + " ]"; 
		playerLabel.color = color[ playerId % color.Length ];

	}

	Color[] color = new Color[]{ Color.gray, Color.red, Color.green, Color.blue, Color.yellow};

	/// <summary>
	/// 観測者に指定
	/// </summary>
	[PunRPC]
	public void SetObserver( bool value )
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
		headModel.enabled = !_isObserver ;
		playerLabel.enabled = !_isObserver;

		cameraImage.SetActive( _isObserver );

		observerController.enabled = _isObserver;

		if( _isObserver )
		{
			head.transform.localPosition = Vector3.zero;
		}
	}		
	private bool _isObserver;
}
