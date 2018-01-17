using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrackedObjects : Photon.MonoBehaviour
{
	[SerializeField]
	private CopyTransform copyTransformHead;

	[SerializeField]
	private Transform lookTarget;

	[SerializeField]
	private CopyTransform copyTransformRightHand;

	[SerializeField]
	private Transform rightHandObject;

	[SerializeField]
	private CopyTransform copyTransformLeftHand;

	[SerializeField]
	private Transform leftHandObject;

	[SerializeField]
	private CopyTransform copyTransformBody;

	[SerializeField]
	private Transform bodyObject;

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
	private IKControl myDrothy;

	public bool forceDisableCopyTransform = false;

	Color[] playerColor = new Color[]{ Color.gray, Color.red, Color.green, Color.blue, Color.yellow};

	public static List<TrackedObjects> list;

	public bool forceDrothy;

	public bool useSimulateFoot = false;

	public bool useKeyboardControl = false;

	void Awake()
	{
		if( list == null )
		{
			list = new List<TrackedObjects>();
		}

		list.Add( this );
	}

	/// <summary>
	/// Initialize & SetObserver より後に呼ばれる前提であるため注意する
	/// </summary>
	void Start () 
	{
		Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

		// 自身でない時の処理
		if( !photonView.isMine  )
		{
			// CopyTransform削除
			var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}

			// プレイヤーIDを取得
			int id = 0;
			var obj = photonView.instantiationData;
			if( obj.Length > 0 )
			{
				id = System.Convert.ToInt32( obj[0] );
			}

			playerLabel.text = "PLAYER[ " + id.ToString() + " ]"; 
			playerLabel.color = playerColor[ id % playerColor.Length ];

			// 観測者でなければドロシー生成
			if( !_isObserver )
			{
				CreateDrothy( id );
			}
		}

		if( forceDisableCopyTransform )
		{
			var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}
		}

		if( forceDrothy ) CreateDrothy();

		if( useKeyboardControl && photonView.isMine ) gameObject.AddComponent<PlayerTest>();

		myCamera.enabled = photonView.ownerId == 0 || photonView.isMine;
		audioListener.enabled = photonView.ownerId == 0 || photonView.isMine; 
		if( headModel != null ) headModel.enabled = !photonView.isMine && !_isObserver;
		playerLabel.enabled = !photonView.isMine && !_isObserver;
	}
	
	// Update is called once per frame
	void Update () 
	{		
		// 自分自身でなくともラベルは全て自分を向く
		if( playerLabel.enabled && Camera.main != null)
		{
			playerLabel.transform.forward = Camera.main.transform.forward;
		}

		if( photonView.ownerId != 0 && !photonView.isMine  ) return;

		// とりあえず仮でRを押して頭の回転リセット
		// TODO この操作をしなくても自動で頭の回転が調整できるようにしたい
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
			this.copyTransformHead.enabled = false;
			this.copyTransformRightHand.enabled = false;
			this.copyTransformLeftHand.enabled = false;
			this.copyTransformBody.enabled = false;
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

		this.copyTransformHead.copySource = copyTransformHead;
		this.copyTransformHead.offsetObject = offsetObject;

		this.copyTransformRightHand.copySource = copyTransformRightHand;
		this.copyTransformRightHand.offsetObject = offsetObject;

		this.copyTransformLeftHand.copySource = copyTransformLeftHand;
		this.copyTransformLeftHand.offsetObject = offsetObject;

		this.copyTransformBody.copySource = copyTransformBody;
		this.copyTransformBody.offsetObject = offsetObject;
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

		copyTransformRightHand.gameObject.SetActive( !_isObserver );
		copyTransformLeftHand.gameObject.SetActive( !_isObserver );
		if( headModel != null )headModel.enabled = !_isObserver ;
		playerLabel.enabled = !_isObserver;

		// 観測者かつ自身ではないときにビジュアルを有効にする
		cameraImage.SetActive( _isObserver && !photonView.isMine );

		observerController.enabled = _isObserver;

		if( _isObserver )
		{
			copyTransformHead.transform.localPosition = Vector3.zero;
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

		if( myDrothy != null )
		{
			Destroy( myDrothy.gameObject );
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

	/// <summary>
	/// ドロシーを生成
	/// </summary>
	private void CreateDrothy( int colorIdx=1 )
	{
		var drothyIK = Instantiate<IKControl>( drothyIKPrefab );

		drothyIK.rightHandObj = rightHandObject;
		drothyIK.leftHandObj = leftHandObject;
		drothyIK.bodyObj = bodyObject;
		drothyIK.lookObj = lookTarget;

		if( useSimulateFoot )
		{
			drothyIK.SetSimulateFoot();
		}

		// TODO プレイヤーIDによってカラバリを変更する
		drothyIK.GetComponent<DrothyController>().SetDressColor( colorIdx );

		myDrothy = drothyIK;
	}
}
