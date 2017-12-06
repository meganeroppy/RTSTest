using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackedObjects : Photon.MonoBehaviour {

	[SerializeField]
	private CopyTransform head;

	[SerializeField]
	private Transform lookTarget;

	[SerializeField]
	private CopyTransform rightHand;

	[SerializeField]
	private CopyTransform leftHand;

	[SerializeField]
	private CopyTransform body;

	[SerializeField]
	private Camera myCamera;

	[SerializeField]
	private AudioListener audioListener;

	[SerializeField]
	private MeshRenderer headModel;

	[SerializeField]
	private GameObject cameraImage;

	[SerializeField]
	private Transform rotateCopyFrom;

	[SerializeField]
	private Transform rotateCopyTo;

	[SerializeField]
	private Text playerLabel;

	[SerializeField]
	private ObserverController observerController;

	[SerializeField]
	private IKControl drothyIKPrefab;

	Color[] playerColor = new Color[]{ Color.gray, Color.red, Color.green, Color.blue, Color.yellow};

	public static List<TrackedObjects> list;

	public bool forceDrothy;

	void Awake()
	{
		if( list == null )
		{
			list = new List<TrackedObjects>();
		}

		list.Add( this );
	}

	// Use this for initialization
	void Start () 
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		// 自身でなければCopyTransformを削除し、ドロシーを生成
		if( !photonView.isMine  )
		{
			var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}

			SetupDrothy();
		}

		if( forceDrothy ) SetupDrothy();

		myCamera.enabled = photonView.ownerId == 0 || photonView.isMine;
		audioListener.enabled = photonView.ownerId == 0 || photonView.isMine; 
		if( headModel != null ) headModel.enabled = !photonView.isMine && !_isObserver;
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
	public void Initialize( GameObject copyTransformHead, GameObject copyTransformRightHand, GameObject copyTransformLeftHand, GameObject copyTransformBody, GameObject offsetObject, int playerId )
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		if( playerId == 0 || photonView.ownerId == 0)
		{
			head.enabled = false;
			rightHand.enabled = false;
			leftHand.enabled = false;
			body.enabled = false;
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

		body.copySource = copyTransformBody;
		body.offsetObject = offsetObject;

		playerLabel.text = "PLAYER[ " + playerId.ToString() + " ]"; 
		playerLabel.color = playerColor[ playerId % playerColor.Length ];
	}

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
		if( headModel != null )headModel.enabled = !_isObserver ;
		playerLabel.enabled = !_isObserver;

		cameraImage.SetActive( _isObserver );

		observerController.enabled = _isObserver;

		if( _isObserver )
		{
			head.transform.localPosition = Vector3.zero;
		}
	}		
	private bool _isObserver;

	/// <summary>
	/// 破棄時お処理
	/// </summary>
	private void OnDestroy()
	{
		if( list != null && list.Contains( this ) )
		{
			list.Remove( this );
		}
	}

	/// <summary>
	/// モデルの回転 自分自身の時はあまり意味がないが、人から見られるときに重要になる
	/// </summary>
	public void UpdateVisualRotation()
	{
		if( rotateCopyFrom != null && rotateCopyTo != null )
		{
			rotateCopyTo.forward = rotateCopyFrom.forward;
		}
	}

	private void SetupDrothy()
	{
		var drothyIK = Instantiate<IKControl>( drothyIKPrefab );

		drothyIK.headObj = head.transform;
		drothyIK.rightHandObj = rightHand.transform;
		drothyIK.leftHandObj = leftHand.transform;
		drothyIK.bodyObj = body.transform;
		drothyIK.lookObj = lookTarget;
	}
}
