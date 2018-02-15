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

	[SyncVar]
	private bool initialized = false;

    /// <summary>
    /// ナビゲーターか？
    /// </summary>
	[SyncVar]
    private bool isNavigator = false;
    private bool isNavigatorPrev = false;
    public bool IsNavigator { get { return isNavigator; } }

    /// <summary>
    /// 参加型ナビゲーターか？
    /// </summary>
    [SyncVar]
    private RtsTestNetworkManager.NavigatorType navigatorType = RtsTestNetworkManager.NavigatorType.Default;
    public RtsTestNetworkManager.NavigatorType NavigatorType { get { return navigatorType; } }

    [SerializeField]
    private IKControl drothyIKPrefab;
    private DrothyController myDrothy;
	public DrothyController MyDrothy{ get{ return myDrothy;}}

    [SerializeField]
    private TextMesh textMesh;

    [SerializeField]
    private GameObject youIcon;

    /// <summary>
    /// 右手でアイテムをつかんでいる時の位置
    /// </summary>
    [SerializeField]
    private Transform holdPosRight;

    /// <summary>
    /// 右手でアイテムをつかんでいる時の位置
    /// </summary>
    [SerializeField]
    private Transform holdPosLeft;

    /// <summary>
    /// 弾が発射される場所右手
    /// </summary>
    [SerializeField]
    private Transform muzzleRight;

    /// <summary>
    /// 弾が発射される場所右手
    /// </summary>
    [SerializeField]
    private Transform muzzleLeft;

    /// <summary>
    /// 銃のビジュアル
    /// </summary>
    [SerializeField]
    private GameObject[] gunsVisual;

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
    /// 右手でつかんでいるアイテム
    /// </summary>
    private DrothyItem holdItemRight = null;

    /// <summary>
    /// 左手でつかんでいるアイテム
    /// </summary>
    private DrothyItem holdItemLeft = null;

    /// <summary>
    /// 頭オブジェクト
    /// 自身にカメライメージがついている
    /// 子要素にいもむしがある
    /// アイテムを食べるときにも使用する
    /// </summary>
    [SerializeField]
    private GameObject headObject = null;
	public GameObject HeadObject{ get { return headObject;} }

    /// <summary>
    /// ナビゲータークラス
    /// </summary>
    private NavigatorController navigatorController;

    /// <summary>
    /// プレイヤーのリスト
    /// サーバーでのみ使用可能
    /// </summary>
    public static List<PlayerTest> list;

    /// <summary>
    /// シーン切り替えにつかうマスク
    /// </summary>
    [SerializeField]
    private MeshRenderer mask;

    /// <summary>
    /// ナビゲーターの数 ただしプレイヤー風ナビゲーターは除外する
    /// </summary>
    public static int PureNavigatorCount {
        get
        {
            if (list == null) return 0;

            int count = 0;
            foreach (PlayerTest p in list)
            {
                if (p.IsNavigator && p.NavigatorType != RtsTestNetworkManager.NavigatorType.Participatory) count++;
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

    /// <summary>
    /// 手の左右判別
    /// </summary>
    private enum HandIndex
    {
        Right,
        Left,
    }
		
    /// <summary>
    /// つかんでいるアイテムと頭との距離がこの値以下になったら食べる
    /// </summary>
    [SerializeField]
	private float eatRange = 0.3f;

	/// <summary>
	/// 置かれているアイテムと手の距離がこれ以下ならつかみ操作で掴める
	/// </summary>
	[SerializeField]
	private float holdRange = 0.8f;

	/// <summary>
	/// 右手のアニメータ
	/// </summary>
	[SerializeField]
	private Animator animRightHand;

	/// <summary>
	/// 左手のアニメータ
	/// </summary>
	[SerializeField]
	private Animator animLeftHand;

	/// <summary>
	/// ドロシーのNetId
	/// </summary>
	[SyncVar]
	private NetworkInstanceId drothyNetId = NetworkInstanceId.Invalid;

	/// <summary>
	/// null参照のドロシーがローカル環境に存在するか？
	/// </summary>
	private static bool unreferencedDrothyExist = false;

    /// <summary>
    /// シーケンス切り替わった時だけ処理を行いたいので現在のシーケンスをプレイヤー側でも保持する
    /// </summary>
    private EventManager.Sequence playerCurrentSequence;

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

		// ローカルプレイヤーでないとき
        if (!isLocalPlayer)
        {
            // カメラ無効（一緒にオーディオリスナーも無効になる）
            var camera = GetComponentInChildren<Camera>();
            if (camera.gameObject) camera.gameObject.SetActive(false);
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
        CmdCreateDrothy(RtsTestNetworkManager.instance.PlayerId);

        // サーバー上でナビゲーターフラグをセットする syncVarなのでのちにクライアントにも反映される
        CmdSetIsNavigator(RtsTestNetworkManager.instance.IsNavigator);

        // サーバー上で参加型フラグをセットする syncVarなのでのちにクライアントにも反映される
        CmdSetIsParticipatory(RtsTestNetworkManager.instance.MyNavigatorType);

        if (RtsTestNetworkManager.instance.IsNavigator)
        {
            // ナビゲーターの場合

            var obj = GameObject.Find("BaseSceneManager");
            if (obj)
            {
                var manager = obj.GetComponent<BaseSceneManager>();
                if (manager != null)
                {
                    manager.ActivatePresetCameras();
                }
            }

			// トラッキングによる制御の有効性を設定する
            if (RtsTestNetworkManager.instance.MyNavigatorType == RtsTestNetworkManager.NavigatorType.Participatory)
            {
                // 参加型の時は有効にする ただし強制キーボード操作の時は無効にする
                trackedObjects.SetEnable(RtsTestNetworkManager.instance.MyInputMode != RtsTestNetworkManager.InputMode.ForceByKeyboard);
            }
            else if (RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByTracking)
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

        // ナビゲーターフラグに変更があった場合のみ処理する
        {
            if (isNavigator != isNavigatorPrev)
            {
                SetNavigatorEnable();
            }
            isNavigatorPrev = isNavigator;
        }

        // 自分の時だけ「YOU]アイコン表示
        youIcon.SetActive(isLocalPlayer);

        // アイテムをつかんでいる時はつかんでいる位置にマイフレーム設定
        // TODO: ローカルとサーバーどちらで行うべきかいまいち定かではないので適宜調整する
        if (isServer)
        {
            UpdateHoldItem();
        }
			
        // アイテム効果タイマーの更新
        if (isServer)
        {
            UpdateItemEffectTimer();
        }

		if( isClient )
		{
			// 自身のドロシーがnull かつ 非参加型ナビゲータでないときに実行
			if( myDrothy == null && ( !IsNavigator || ( IsNavigator && NavigatorType == RtsTestNetworkManager.NavigatorType.Participatory ) ) )
			{
				Debug.Log( netId.ToString() + "のプレイヤーのドロシーがnull参照なのでフラグを立てた" );

				unreferencedDrothyExist = true;
			}

			// 非ローカルからは呼べないのでローカルのプレイヤーから呼び出す
			if( isLocalPlayer && unreferencedDrothyExist )
			{
				Debug.Log( "フラグがたっているのでNetId = " + netId.ToString() + "のローカルプレイヤーからドロシーの参照を要求" );
				CmdRequestDrothyReference();
			}
		}

        /////////////////////////////////////////
        // ■ここから↓はローカルプレイヤーのみ■
        /////////////////////////////////////////

        if (!isLocalPlayer)
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

        // シーケンスが変わった時だけビジュアルの更新を行う
        if (playerCurrentSequence != EventManager.instance.CurrentSequence)
        {
            UpdateHandVisual();
        }

        playerCurrentSequence = EventManager.instance.CurrentSequence;
    }

    /// <summary>
    /// ナビゲーター設定の有効を切り替え
    /// </summary>
    private void SetNavigatorEnable()
    {
        if (navigatorController == null) navigatorController = GetComponent<NavigatorController>();

        navigatorController.enabled = isNavigator;

		// 非参加型ナビゲータかつ自身でないときにいもむしビジュアルを有効にする
		headObject.SetActive(isNavigator && navigatorType == RtsTestNetworkManager.NavigatorType.Default && !isLocalPlayer);

        // トラッキングによる移動の有効を設定 結構難解なので間違いがないか再三確認する
        bool enableTracking;

        if (isNavigator)
        {
            // ナビゲーターの場合
			if (navigatorType == RtsTestNetworkManager.NavigatorType.Participatory)
            {
                // 参加型ナビゲーターの場合は有効 ただし強制キーボード操作の時は無効
				// TODO: RtsTestNetworkManager.instance.MyInputModeつかってるけどやばくない？
                enableTracking = RtsTestNetworkManager.instance.MyInputMode != RtsTestNetworkManager.InputMode.ForceByKeyboard;
            }
			// TODO: RtsTestNetworkManager.instance.MyInputModeつかってるけどやばくない？
            else if (RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByTracking)
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
			// TODO: RtsTestNetworkManager.instance.MyInputModeつかってるけどやばくない？
            if (RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByKeyboard)
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

	bool grabbingRight = false;
	bool grabbingLeft = false;

	[Client]
	private void CheckInput()
	{
		// これを呼ばないとOVRInputのメソッドが動かないらしいので呼ぶ
		OVRInput.Update();

		var grabRight = OVRInput.Get (OVRInput.RawAxis1D.RIndexTrigger) > 0 || Input.GetKey(KeyCode.J);
		var grabLeft = OVRInput.Get (OVRInput.RawAxis1D.LIndexTrigger) > 0 || Input.GetKey(KeyCode.H);

		// アイテムをつかむ
		{
			// 右手
			{
				bool grabbingRightPrev = grabbingRight;

				if ( (grabRight && !grabbingRight) )
				{
					grabbingRight = true;
					Debug.Log ("右手をにぎる");
					CmdSetHoldItem (HandIndex.Right);
				}  
				else if (!grabRight && grabbingRight) 
				{
					grabbingRight = false;
					Debug.Log ("右手をひらく");

					// アイテムをつかんでいたら離す
					CmdReleaseHoldItem(HandIndex.Right);
				}

				// アニメーション更新
				if( grabbingRightPrev != grabbingRight )
				{
					var animName = grabbingRight ? "GrabSmall" : "Natural";
					animRightHand.SetTrigger(animName);
				}
			}

			// 左手
			{
				bool grabbingLeftPrev = grabbingLeft;

				if ( (grabLeft && !grabbingLeft) )
				{
					grabbingLeft = true;
					Debug.Log ("左手をにぎる");
					CmdSetHoldItem (HandIndex.Left);
				}
				else if (!grabLeft && grabbingLeft)
				{				
					grabbingLeft = false;
					Debug.Log ("左手をひらく");

					// アイテムをつかんでいたら離す
					CmdReleaseHoldItem(HandIndex.Left);
				}

				// アニメーション更新
				if( grabbingLeftPrev != grabbingLeft )
				{
					var animName = grabbingLeft ? "GrabSmall" : "Natural";
					animLeftHand.SetTrigger(animName);
				}
			}
		}

        // アイテムを食べる
        {
            // コントローラ操作による食事操作シミュレート
            if (RtsTestNetworkManager.instance.MyInputMode == RtsTestNetworkManager.InputMode.ForceByKeyboard)
            {
                if (Input.GetKey(KeyCode.Y) || Input.GetKey(KeyCode.U))
                {
                    CmdSimulateEatWithTouch(true);
                }               
                else
                {
                    CmdSimulateEatWithTouch(false);
                }
            }
        }

        // ナビゲーターになる

        // 現状いらなそう 必要になったら実装する
		if (Input.GetKeyDown(KeyCode.N) ||
            OVRInput.GetDown(OVRInput.RawButton.RThumbstick) // 右スティック押し込み
        )
        {
            Debug.LogWarning("ナビゲーターとプレイヤー切り替え機能は未実装");
        }

        // ナビゲーターモード切り替え
        // 現状いらなそう 必要になったら実装する
        if (Input.GetKeyDown(KeyCode.M) ||
            OVRInput.GetDown(OVRInput.RawButton.LThumbstick) // 左スティック押し込み
        )
        {
            Debug.LogWarning("ナビゲーターモード切り替え機能は未実装");
        }
    }

    /// <summary>
    /// 手のビジュアルを更新する
    /// 最後のシーンだけ銃を表示、それ以外のシーンの時は手を表示する
    /// </summary>
    private void UpdateHandVisual()
    {
        var inFinalScene =
            EventManager.instance.CurrentSequence == EventManager.Sequence.TeaRoomSmall ||
            EventManager.instance.CurrentSequence == EventManager.Sequence.PopMushrooms_Event ||
            EventManager.instance.CurrentSequence == EventManager.Sequence.Ending_Event;

        // 手
        animRightHand.gameObject.SetActive(!inFinalScene);
        animLeftHand.gameObject.SetActive(!inFinalScene);

        // 銃
        foreach( GameObject g in gunsVisual )
        {
            g.SetActive(inFinalScene);
        }
    }

    /// <summary>
    /// つかんでいるアイテムの状態を更新
    /// </summary>    
    [Server]
    private void UpdateHoldItem()
    {
        // アイテムの位置を該当する手の位置と回転に更新
        if (holdItemRight != null)
        {
//            Debug.LogWarning("右手位置に更新");
            holdItemRight.transform.position = holdPosRight.position;
			holdItemRight.transform.rotation = holdPosRight.rotation;

            // 頭との距離が閾値以下になったらアイテムを食べる
            if( Vector3.Distance( holdItemRight.transform.position, headObject.transform.position) < eatRange )
            {
                EatItem( HandIndex.Right );
            }
        }

        if (holdItemLeft != null)
        {
//            Debug.LogWarning("左手位置に更新");
			holdItemLeft.transform.position = holdPosLeft.position;
			holdItemLeft.transform.rotation = holdPosLeft.rotation;

			// 頭との距離が閾値以下になったらアイテムを食べる
            if (Vector3.Distance(holdItemLeft.transform.position, headObject.transform.position) < eatRange)
            {
                EatItem(HandIndex.Left);
            }
        }
    }

    /// <summary>
    /// ドロシーを生成する
    /// </summary>
    [Command]
    private void CmdCreateDrothy(int playerId)
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
		// TODO: RtsTestNetworkManager.instance.UseSimulateFootつかってるけどやばくない？
        if (RtsTestNetworkManager.instance.UseSimulateFoot)
        {
            drothyIK.SetSimulateFoot();
        }

        // プレイヤーIDによってカラバリを変更する
        drothyIK.GetComponent<DrothyController>().ColorIdx = playerId;

        myDrothy = drothyIK.GetComponent<DrothyController>();
        if (myDrothy == null)
        {
            return;
        }

        myDrothy.SetOwner(trackedObjects.BodyObject);

        // どちらが正しいかはまだ不明
        //      NetworkServer.Spawn(drothy.gameObject);
        NetworkServer.SpawnWithClientAuthority(myDrothy.gameObject, gameObject);

        RpcPassDrothyReference(myDrothy.netId);

		// ドロシーのNetIdをサーバー側で保持する
		drothyNetId = myDrothy.netId;
    }

    /// <summary>
    /// クライアント側でもドロシーの参照を持たせる
    /// </summary>
    [ClientRpc]
	private void RpcPassDrothyReference(NetworkInstanceId drothyNetId)
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod() + "NetId = " + drothyNetId);

        var drothyObj = ClientScene.FindLocalObject(drothyNetId);
		if( !drothyObj )
		{
			Debug.Log( "NetId = " + netId.ToString() + "のプレイヤーからみて" + drothyNetId.ToString() + "のドロシーはみつからなかった");
			return;
		}
		Debug.Log( "ドロシー発見！NetId = " + netId.ToString() + "のプレイヤーのドロシーは" + drothyNetId.ToString() + "のドロシーだった");

        myDrothy = drothyObj.GetComponent<DrothyController>();

        // まだクライアント側はIKターゲットが未指定なのでセットする
        {
            var drothyIK = myDrothy.GetComponent<IKControl>();
            if (drothyIK != null)
            {
                drothyIK.rightHandObj = trackedObjects.RightHandObject;
                drothyIK.leftHandObj = trackedObjects.LeftHandObject;
                drothyIK.bodyObj = trackedObjects.BodyObject;
                drothyIK.lookObj = trackedObjects.LookTarget;
            }
        }

        // ドロシーの表示設定
        bool drothyVisible;
		// TODO: RtsTestNetworkManager.instance.ForceDisplayDrothyつかってるけどやばくない？
        if (RtsTestNetworkManager.instance.ForceDisplayDrothy)
        {
            // 強制ドロシー表示フラグがあったら確実に表示する
            drothyVisible = true;
        }
        else if (isNavigator)
        {
            // ナビゲーターの時
            if (isLocalPlayer)
            {
				// ローカルの時(自分のとき)は表示しない
                drothyVisible = false;
            }
            else
            {
                // リモートかつ参加型の時は表示する、そうでなければ表示しない 
				drothyVisible = navigatorType == RtsTestNetworkManager.NavigatorType.Participatory;
            }
        }
        else
        {
            // プレイヤーの時

            // ローカルでなければ表示する
            drothyVisible = !isLocalPlayer;
        }

        drothyObj.SetActive(drothyVisible);

		unreferencedDrothyExist = false;
    }

	/// <summary>
	/// ドロシーの参照を要求する
	/// 生成済みの全てのプレイヤーに対して実行される
	/// ただし非参加型ナビゲータは除外
	/// </summary>
	[Command]
	private void CmdRequestDrothyReference()
	{
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		var players = GameObject.FindGameObjectsWithTag("Player");

		Debug.Log("生成済みプレイヤー数 -> " + players.Length.ToString());

		foreach( GameObject g in players )
		{
			var p = g.GetComponent<PlayerTest>();
			if( p == null ) continue;

			// 非参加型ナビゲータを除外
			if( p.IsNavigator && p.NavigatorType == RtsTestNetworkManager.NavigatorType.Default )
			{
				Debug.Log( "NetId = " + p.netId.ToString() + "のプレイヤーは非参加型ナビゲータなので除外");
				continue;
			}
			// ドロシーNetIdが未設定の時は除外
			if( p.drothyNetId == NetworkInstanceId.Invalid ){
				Debug.Log( "NetId = " + p.netId.ToString() + "のプレイヤーはドロシーのNetIdが未設定なので除外");

			continue;
			}

			p.RpcPassDrothyReference( p.drothyNetId );
		}
	}

    /// <summary>
    /// ナビゲーターフラグを設定する
    /// </summary>
    [Command]
    private void CmdSetIsNavigator(bool value)
    {
        isNavigator = value;
    }

    /// <summary>
    /// 参加型フラグを設定する
    /// </summary>
    [Command]
    private void CmdSetIsParticipatory(RtsTestNetworkManager.NavigatorType value)
    {
        navigatorType = value;
    }

    /// <summary>
    /// アイテムをつかむ
    /// サーバー処理
    /// </summary>
    [Command]
    private void CmdSetHoldItem( HandIndex hIndex )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// 対象となる手を取得
		var targetHand = hIndex == HandIndex.Right ? holdPosRight : holdPosLeft;

		// 範囲内に掴めるアイテムがあるか調べる
		// 複数存在した場合はもっとも近くにあるものを取得する
		DrothyItem selectedItem = null;
		var items = EventManager.instance.ItemList;
		if( items == null )
		{
			Debug.Log("アイテムリストなし");
			return;
		}

		float nearest = float.MaxValue;
		for( int i=0 ; i< items.Count ; ++i )
		{
            var item = items[i];
            if (item == null) continue;

			float distance = Vector3.Distance( targetHand.position, item.transform.position );

			// 掴める範囲外はコンティニュー
			if( distance > holdRange ) 
			{
				Debug.Log(item.name + "はつかみ可能範囲外( 距離 : " + distance.ToString() + ")");
				continue;
			}

			// 暫定最短距離以下か？
			if( distance < nearest )
			{
				// 暫定最短距離を更新
				nearest = distance;
				selectedItem = item;
			}
		}

        // つかみ候補が無ければなにもしない
		if ( selectedItem == null )
        {
            Debug.Log("つかみ候補なし");
            return;
        }

        // つかめる状態でない（他のプレイヤーがつかんでいるなど）ときはなにもしない
		if( !selectedItem.Holdable )
        {
            Debug.Log("つかめる状態でない");
            return;
        }

        // すでにものをつかんでいる手でつかもうとしたらなにもしない
        if ( ( hIndex == HandIndex.Right && holdItemRight != null ) || (hIndex == HandIndex.Left && holdItemLeft != null))
        {
            Debug.Log((hIndex == HandIndex.Right ? "右手" : "左手") + "はすでにアイテムをつかんでいる");
			return;
        }

        if(hIndex == HandIndex.Right)
        {
			holdItemRight = selectedItem;
        }
        else
        {
			holdItemLeft = selectedItem;
        }
			
        // つかむ
		selectedItem.SetHeld(true);

        Debug.Log((hIndex == HandIndex.Right ? "右手" : "左手") + "でアイテムをつかんだ");
    }

	/// <summary>
	/// アイテムを持っていたら離す
	/// </summary>
	[Command]
	private void CmdReleaseHoldItem( HandIndex hIndex )
	{
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		if( (hIndex == HandIndex.Right && holdItemRight == null) || (hIndex == HandIndex.Left && holdItemLeft == null) )
		{
			Debug.Log((hIndex == HandIndex.Right ? "右手" : "左手") + "にはアイテムを持っていない");
			return;
		}

		if( hIndex == HandIndex.Right )
		{
			Debug.Log("右手のアイテムを離した");
			holdItemRight.SetHeld(false);
			holdItemRight = null;
		}
		else
		{
			Debug.Log("左手のアイテムを離した");
			holdItemLeft.SetHeld(false);
			holdItemLeft = null;
		}
	}

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    [Server]
    private void EatItem(HandIndex hIndex)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var target = hIndex == HandIndex.Right ? holdItemRight : holdItemLeft;

        // アイテムを持っていなかったらなにもしない
        if ( target == null )
        {
            Debug.Log((hIndex == HandIndex.Right ? "右手" : "左手") + "はアイテムをつかんでいない");
            return;
        }

        // アイテムを食べられるタイミングでなかったらなにもしない
        if (EventManager.instance.CurrentSequence != EventManager.Sequence.PopCakes1_Event &&
            EventManager.instance.CurrentSequence != EventManager.Sequence.PopCakes2_Event &&
            EventManager.instance.CurrentSequence != EventManager.Sequence.PopMushrooms_Event
        )
        {
            Debug.Log("今はその時ではない");
            return;
        }

        // たべる
        target.CmdEaten();

        // アイテムの効果を得る
        itemEffectTimer = target.EffectTime;

        // アイテムをもっていない状態にする
        if( hIndex == HandIndex.Right )
        {
            holdItemRight = null;
        }
        else
        {
            holdItemLeft = null;
        }

        Debug.Log((hIndex == HandIndex.Right ? "右手" : "左手") + "のアイテムを食べた");
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
        if (broadcastTrigger == null)
            broadcastTrigger = GetComponent<Dissonance.VoiceBroadcastTrigger>();

        if (broadcastTrigger == null || myDrothy == null) return;

        if (broadcastTrigger.IsTransmitting)
        {
            myDrothy.CmdTalk();
        }
    }

    /// <summary>
    /// キーボード操作でTouchでアイテムを食べる操作を再現
    /// 両手は( 0, 0, 0 ) 頭は( 0, 1, 0 )の位置なので両手の位置を上げる
    /// </summary>
	[Command]
    private void CmdSimulateEatWithTouch( bool on )
    {
        holdPosRight.localPosition = holdPosLeft.localPosition = Vector3.up * ( on ? 1f : 0 );
    }

    /// <summary>
    /// カメラマスクの色を変更する
    /// シーン切り替え時用
    /// </summary>
    [ClientRpc]
    public void RpcSetCameraMaskColor(Color color)
    {
        mask.material.color = color;
    }
}
