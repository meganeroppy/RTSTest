using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの入力による制御とサーバーとの同期を管理
/// </summary>
public class PlayerTest : NetworkBehaviour
{
    private TrackedObjects trackedObjects;

    [SerializeField]
    private float moveSpeed = 10f;
    [SerializeField]
    private float rotSpeed = 10f;

    /// <summary>
    /// 管理者か？
    /// </summary>
	[SyncVar]
	private bool isObserver = false;
    private bool isObserverPrev = false;
    public bool IsObserver { get { return isObserver; } }

    /// <summary>
    /// 削除予定
    /// 観測者だと赤くなるだけのキューブ
    /// </summary>
    [SerializeField]
	private MeshRenderer observerSign;

    [SerializeField]
    private IKControl drothyIKPrefab;
    private DrothyController myDrothy;

    [SerializeField]
    private TextMesh textMesh;

    [SerializeField]
    private GameObject youIcon;

    /// <summary>
    /// アイテムをつかんでいる時のアイテムの位置
    /// </summary>
    [SerializeField]
    private Transform holdPos;

    /// <summary>
    /// 現在のドロシーのスケール
    /// </summary>
    [SyncVar]
    private float drothyScale = 1f;

    /// <summary>
    /// （仮）プリセットのプレイヤーカラー
    /// </summary>
    Color[] playerColor = new Color[] { Color.gray, Color.red, Color.green, Color.blue, Color.yellow };

    /// <summary>
    /// 情報表示ラベル
    /// </summary>
    [SerializeField]
    private Text playerLabel;

    /// <summary>
    /// つかめる距離にあるアイテム
    /// </summary>
    private DrothyItem holdTarget = null;

    /// <summary>
    /// つかんでいるアイテム
    /// </summary>
    private DrothyItem holdItem = null;

    /// <summary>
    /// カメライメージ
    /// </summary>
    [SerializeField]
    private GameObject cameraImage;

    /// <summary>
    /// 観測者クラス
    /// </summary>
    private ObserverController observerController;

    /// <summary>
    /// プレイヤーのリスト
    /// サーバーでのみ使用可能
    /// </summary>
    public static List<PlayerTest> list;

	/// <summary>
	/// 観測者の数 ただしプレイヤー風観測者は除外する
	/// </summary>
	public static int PureObserverCount{
		get
		{
			if( list == null ) return 0;

			int count = 0;
			foreach( PlayerTest p in list )
			{
				if( p.IsObserver ) count++;
			}

			return count;
		}
	}

    /// <summary>
    /// アイテムの効果が発生している時間
    /// </summary>
    private float itemEffectTimer = 0;
    public float ItemEffectTimer { get { return itemEffectTimer; } }

    /// <summary>
    /// 音声送信トリガー
    /// </summary>
    private Dissonance.VoiceBroadcastTrigger broadcastTrigger;

    private void Awake()
    {
		trackedObjects = GetComponent<TrackedObjects>();
    }

    // Use this for initialization
    void Start()
    {
		if (isServer)
		{
			if (list == null) list = new List<PlayerTest>();
			list.Add(this);

			Debug.Log("AddPlayer : プレイヤー数 -> " + list.Count.ToString());
		}

        if (!isLocalPlayer)
        {
            // カメラ無効（一緒にオーディオリスナーも無効になる）
            var camera = GetComponentInChildren<Camera>();
            if (camera.gameObject) camera.gameObject.SetActive(false);

            // 観測者かつ自身ではないときにビジュアルを有効にする
            cameraImage.SetActive(isObserver && !isLocalPlayer);
        }

        // ラベルの設定
        { 
            int id = 0;

			var nIdentity = GetComponent<NetworkIdentity>();
            if (nIdentity != null)
            {
                //    id = nIdentity.netId;
            }

            playerLabel.text = "PLAYER[ " + id.ToString() + " ]";
            playerLabel.color = playerColor[id % playerColor.Length];
        }
    }

    /// <summary>
    /// ローカルプレイヤーとして開始したときに呼ばれる
    /// </summary>
    public override void OnStartLocalPlayer()
    {
    //    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// プレイヤー名を変更する
		gameObject.name = "[YOU] " + gameObject.name;

        // サーバー上でドロシーを生成
		CmdCreateDrothy( RtsTestNetworkManager.instance.PlayerId );

        // サーバー上で観測者フラグをセットする syncVarなのでのちにクライアントにも反映される
        CmdSetIsObserver(RtsTestNetworkManager.instance.IsObserver);

        if(RtsTestNetworkManager.instance.IsObserver)
        {
            // 観測者の場合

            var obj = GameObject.Find("BaseSceneManager");
            if( obj )
            {
                var manager = obj.GetComponent<BaseSceneManager>();
                if( manager != null )
                {
                    manager.ActivatePresetCameras();
                }
            }

			if( RtsTestNetworkManager.instance.MyObserverType == RtsTestNetworkManager.ObserverType.Participatory )
			{
                // 参加型の時は有効にする
                trackedObjects.SetEnable(true);
			}
			else if( RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByTracking )
            {
                // 強制トラッカー依存操作フラグがあったらコンポーネントを有効にする
                trackedObjects.SetEnable(true);
            }
			else
			{
                // そうでなければ無効にする
                trackedObjects.SetEnable(false);
			}
        }
		else
		{
            // 通常プレイヤーの場合 強制キーボード操作フラグがなければトラッキングによる制御
            trackedObjects.SetEnable(RtsTestNetworkManager.instance.MyInputMode != RtsTestNetworkManager.InputMode.ForceByKeyboard);
		}
    }

    public override void OnStartClient()
    {
    //    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
    }

    // Update is called once per frame
    void Update()
    {
        // 自分自身でなくともラベルは全て自分を向く
        if (playerLabel.enabled && Camera.main != null)
        {
            playerLabel.transform.forward = Camera.main.transform.forward;
        }

        // ラベルを更新
        textMesh.text = netId.Value.ToString();

        // 観測者フラグに変更があった場合のみ処理する
        {
            if (isObserver != isObserverPrev)
            {
				SetObserverEnable();
            }
            isObserverPrev = isObserver;
        }

        // 自分の時だけ「YOU]アイコン表示
        youIcon.SetActive(isLocalPlayer);

        // アイテムをつかんでいる時はつかんでいる位置にマイフレーム設定
        if (holdItem)
        {
            holdItem.transform.position = holdPos.position;
        }

        if (isClient)
        {
            // サーバーの値が同期されるdrothyScaleの値でドロシーのスケールを更新
            if (myDrothy != null)
            {
                myDrothy.transform.localScale = Vector3.one * drothyScale;
            }
        }

        // アイテム効果タイマーの更新
        if (isServer)
        {
            UpdateItemEffectTimer();
        }

        /////////////////////////////////////////
        // ■ここから↓はローカルプレイヤーのみ■
        /////////////////////////////////////////

        if ( !isLocalPlayer )
		{
			return;
		}

		// 強制キーボード操作の時の操作
		// TODO: オブザーバーの操作と一緒でいい気がする 
		{
			if (RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByKeyboard)
	        {
	            Vector3 move = new Vector3(
	                Input.GetAxisRaw("Horizontal"),
	                0,
	                Input.GetAxisRaw("Vertical"));

	            transform.Translate(move * moveSpeed * Time.deltaTime);
	            //	myRigidbody.velocity = (transform.forward + move.normalized) * fSpeed ;

	            var rot = Input.GetKey(KeyCode.I) ? -1 : Input.GetKey(KeyCode.O) ? 1 : 0;
	            if (rot != 0)
	            {
	                transform.Rotate(Vector3.up * rot * rotSpeed * Time.deltaTime);
	            }
	        }
		}			

		CheckInput();

        CheckTalking();

		// つかみ候補アイテムから一定距離はなれたら候補から外す
        // サーバーのみ
        if ( isServer && holdTarget && Vector3.Distance( transform.position, holdTarget.transform.position ) > 5f)
        {
            holdTarget = null;
        }
    }

	/// <summary>
	/// 観測者設定の有効を切り替え
	/// </summary>
	private void SetObserverEnable()
	{
		// 削除候補
		// 観測者サインの色を変える
		observerSign.material.color = isObserver ? Color.red : Color.white;

		if (observerController == null) observerController = GetComponent<ObserverController>();

		observerController.enabled = isObserver;

		// 観測者かつ自身ではないときにビジュアル（いもむし）を有効にする
		// TODO: 参加型観測者の見た目をいもむしで無くす場合は要処理変更
		cameraImage.SetActive(isObserver && !isLocalPlayer);

		// トラッキングによる移動の有効を設定 結構難解なので間違いがないか再三確認する
		bool enableTracking;

		if( isObserver )
		{
			// 観測者の場合
			if( RtsTestNetworkManager.instance.MyObserverType == RtsTestNetworkManager.ObserverType.Participatory )
			{
				// 参加型観測者の場合は有効
				enableTracking = true;
			}
			else if( RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByTracking )
			{
				// 強制トラッカー依存操作の時は有効
				enableTracking = true;
			}
			else
			{
				// 上記のどちらでもなければ無効
				enableTracking = false;
			}
		}
		else
		{
			// プレイヤーの場合
			if( RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByKeyboard )
			{
				// 強制キーボード操作の時は無効
				enableTracking = false;
			}
			else
			{
				// そうでなければ有効
				enableTracking = true;
			}
		}

        trackedObjects.SetEnable(enableTracking);	
	}

	[Client]
	private void CheckInput()
	{
		
		// アイテムをつかむ
		// TODO: タッチでの操作は追追改善する
		if( Input.GetKeyDown(KeyCode.H) ||
			OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) || // 右人差し指トリガー
			OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger) // 左人差し指トリガー
		)
		{
			CmdSetHoldItem();
		}

		// アイテムを食べる
		if( Input.GetKeyDown(KeyCode.Y) ||
			OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) || // 右中指トリガー
			OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) // 左中指トリガー
		)
		{
			CmdEatItem();
		}	
	
		// 観測者になる
		// 現状いらなそう 必要になったら実装する
		if (Input.GetKeyDown(KeyCode.O) || 
			OVRInput.GetDown(OVRInput.RawButton.RThumbstick) // 右スティック押し込み
		)
		{
			Debug.LogWarning("観測者とプレイヤー切り替え機能は未実装");
		}

		// 観測者モード切り替え
		// 現状いらなそう 必要になったら実装する
		if (Input.GetKeyDown(KeyCode.M) || 
			OVRInput.GetDown(OVRInput.RawButton.LThumbstick) // 左スティック押し込み
		)
		{
			Debug.LogWarning("観測者モード切り替え機能は未実装");
		}
	}

    /// <summary>
    /// ドロシーを生成する
    /// </summary>
    [Command]
	private void CmdCreateDrothy( int playerId )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        // プレハブから生成
        var drothyIK = Instantiate<IKControl>(drothyIKPrefab);

        // IKのターゲットをトラッキングオブジェクトから取得
        {
            drothyIK.rightHandObj = trackedObjects.RightHandObject;
            drothyIK.leftHandObj = trackedObjects.LeftHandObject;
            drothyIK.bodyObj = trackedObjects.BodyObject;
            drothyIK.lookObj = trackedObjects.LookTarget;
        }

        // 設定が有効になっている場合は脚の動きのシミュレートを行う
		if (RtsTestNetworkManager.instance.UseSimulateFoot)
        {
            drothyIK.SetSimulateFoot();
        }
			
        // プレイヤーIDによってカラバリを変更する
		drothyIK.GetComponent<DrothyController>().ColorIdx = playerId;

        myDrothy = drothyIK.GetComponent<DrothyController>();
        if( myDrothy == null )
        {
            return;
        }

        myDrothy.SetOwner(trackedObjects.BodyObject);

        // どちらが正しいかはまだ不明
//      NetworkServer.Spawn(drothy.gameObject);
        NetworkServer.SpawnWithClientAuthority(myDrothy.gameObject, gameObject);

        RpcPassDrothyReference(myDrothy.netId);
    }

    /// <summary>
    /// クライアント側でもドロシーの参照を持たせる
    /// </summary>
    [ClientRpc]
    private void RpcPassDrothyReference( NetworkInstanceId netId )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		var drothyObj = ClientScene.FindLocalObject(netId);

        myDrothy = drothyObj.GetComponent<DrothyController>();

        // まだクライアント側はIKターゲットが未指定なのでセットする
        {
            var drothyIK = myDrothy.GetComponent<IKControl>();
            if ( drothyIK != null)
            {
                drothyIK.rightHandObj = trackedObjects.RightHandObject;
                drothyIK.leftHandObj = trackedObjects.LeftHandObject;
                drothyIK.bodyObj = trackedObjects.BodyObject;
                drothyIK.lookObj = trackedObjects.LookTarget;
            }
        }

        // 自分のドロシーは無効にする
		bool drothyVisible;
		if( RtsTestNetworkManager.instance.ForceDisplayDrothy )
		{
			// 強制ドロシー表示フラグがあったら確実に表示する
			drothyVisible = true;
		}
		else if( isObserver )
		{
			// 管理者の時
			if( isLocalPlayer )
			{
				// ローカルの時は表示しない
				drothyVisible = false;
			}
			else
			{
				// リモートかつ参加型の時は表示し、そうでなければ表示しない 
				// 表示しない場合はいもむしが表示される
				drothyVisible = RtsTestNetworkManager.instance.MyObserverType == RtsTestNetworkManager.ObserverType.Participatory;
			}
		}
		else
		{
			// プレイヤーの時
			// ローカルでなければ表示する
			drothyVisible = !isLocalPlayer;
		}
		drothyObj.SetActive( drothyVisible );
    }

    /// <summary>
    /// 観測者フラグを設定する
    /// </summary>
    [Command]
	private void CmdSetIsObserver( bool value )
	{
        isObserver = value;
    }

    /// <summary>
    /// サーバーでのみ衝突を判定する
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
    //    if ((isObserver ) || !isLocalPlayer ) return;

        if (other.tag.Equals("Item"))
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod() + other.name);

            var item = other.GetComponent<DrothyItem>();
            if (item != null)
            {
                holdTarget = item;
                SetHoldTarget(item.netId);
            } 
        }
    }

    /// <summary>
    /// つかみ候補のアイテムのセット
    /// </summary>
    [Server]
    private void SetHoldTarget( NetworkInstanceId id )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var obj = NetworkServer.FindLocalObject(id);
        if (!obj) return;
        var item = obj.GetComponent<DrothyItem>();
        if( item != null )
        {
            holdTarget = item;
        }
    }

    /// <summary>
    /// つかんでいるアイテムをサーバーにも反映する
    /// </summary>
    [Command]
    private void CmdSetHoldItem()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
        if (!holdTarget) return;

        holdItem = holdTarget.GetComponent<DrothyItem>();

        holdTarget = null;

        // つかんでいるプレイヤーに権限を与える
        var nIdentity = holdItem.GetComponent<NetworkIdentity>();
        if (nIdentity != null && !nIdentity.hasAuthority)
        {
            nIdentity.AssignClientAuthority(connectionToClient);
        }
    }
    
    /// <summary>
    /// アイテムを消費する
    /// </summary>
    [Command]
    private void CmdEatItem( )
    {
        // アイテムを持っていなかったらなにもしない
        if (!holdItem) return;

        // アイテムを食べられるタイミングでなかったらなにもしない
        if (EventManager.instance.CurrentSequence != EventManager.Sequence.PopCakes1_Event &&
            EventManager.instance.CurrentSequence != EventManager.Sequence.PopCakes2_Event &&
            EventManager.instance.CurrentSequence != EventManager.Sequence.PopMushrooms_Event
        )
            return;            

        // たべる
        holdItem.CmdEaten();

        // アイテムの効果を得る
        itemEffectTimer = holdItem.EffectTime;

        holdItem = null;
    }

    /// <summary>
    /// アイテム効果タイマーを減少させる
    /// </summary>
    [Server]
    private void UpdateItemEffectTimer()
    {
        if( itemEffectTimer > 0 )
        {
            itemEffectTimer -= Time.deltaTime;
        }
    }

    [Server]
    private void ChangeScale()
    {
        drothyScale = 10f;
    }

    /// <summary>
    /// サーバーの時は削除時にリストから自身を削除
    /// </summary>
    [ServerCallback]
    private void OnDestroy()
    {
        if( list != null && list.Contains(this) )
        {
            list.Remove(this);
            Debug.Log("プレイヤーリストから自身を削除 -> プレイヤー数 " + list.Count.ToString() );
        }
    }

    /// <summary>
    /// ＊要テスト＊
    /// プレイヤーが発言している時はドロシーを口パクさせる
    /// </summary>
    [Client]
    void CheckTalking()
    {
        if( broadcastTrigger == null )
            broadcastTrigger = GetComponent<Dissonance.VoiceBroadcastTrigger>();

        if (broadcastTrigger == null || myDrothy == null) return;

        if( broadcastTrigger.IsTransmitting )
        {
            myDrothy.CmdTalk();
        }
    }
}