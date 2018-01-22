using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの入力による制御とサーバーとの同期を管理
/// </summary>
public class PlayerTest : NetworkBehaviour
{
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

    /// <summary>
    /// 脚の動きのシミュレーションを有効にするか？
    /// </summary>
    [SerializeField]
    private bool useSimulateFoot = false;

    /// <summary>
    /// 自身であってもドロシーを表示する
    /// </summary>
    [SerializeField]
    private bool forceDisplayDrothy = false;

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
    /// 削除候補
    /// NetId
    /// Cmdでサーバー側にセットしなくても勝手に同期されるかも？
    /// </summary>
    [SyncVar]
    private string netIdStr;

    /// <summary>
    /// アイテムをつかんでいる時のアイテムの位置
    /// </summary>
    [SerializeField]
    private Transform holdPos;

    /// <summary>
    /// 削除候補
    /// 現在食べた回数
    /// </summary>
    [SyncVar]
    private int eatCount = 0;

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
    /// キーボード操作を有効にするか？
    /// </summary>
    public bool forceUseKeyboardControl = false;

    /// <summary>
    /// 情報表示ラベル
    /// </summary>
    [SerializeField]
    private Text playerLabel;

    /// <summary>
    /// つかめる距離にあるアイテム
    /// </summary>
    private Mushroom holdTarget = null;

    /// <summary>
    /// つかんでいるアイテム
    /// </summary>
    private Mushroom holdItem = null;

    /// <summary>
    /// カメライメージ
    /// </summary>
    [SerializeField]
    private GameObject cameraImage;

    /// <summary>
    /// 観測者クラス
    /// </summary>
    private ObserverController observerController;

    [SyncVar]
    private bool biggenFlag = false;

    // Use this for initialization
    void Start()
    {
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
            //	var obj = photonView.instantiationData;
            //	if( obj.Length > 0 )
            //	{
            //		id = System.Convert.ToInt32( obj[0] );
            //	}

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
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// プレイヤー名を変更する
		gameObject.name = "[YOU] " + gameObject.name;

        // サーバー上でドロシーを生成
        CmdCreateDrothy();

        // ＊不要説あり＊ NetIDをサーバーにセット 
        CmdSetNetIdStr();

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

            if( RtsTestNetworkManager.instance.ForceRelatedToTracking)
            {
                var to = GetComponent<TrackedObjects>();
                to.SetEnable(true);
            }
        }
		else
		{
            // 通常プレイヤーの場合

			var to = GetComponent<TrackedObjects>();
			to.SetEnable(!forceUseKeyboardControl);
		}
    }

    /// <summary>
    /// 不要説
    /// </summary>
    [Command]
    private void CmdSetNetIdStr()
    {
        netIdStr = netId.Value.ToString();
    }

    public override void OnStartClient()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
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
    //    textMesh.text = netId.Value.ToString();
        textMesh.text = netIdStr;

        // 観測者フラグに変更があった場合のみ処理する
        {
            if (isObserver != isObserverPrev)
            {
                observerSign.material.color = isObserver ? Color.red : Color.white;

                if (observerController == null)
                {
                    observerController = GetComponent<ObserverController>();
                }

                if (observerController != null)
                {
                    observerController.enabled = isObserver;
                }

				// 観測者かつ自身ではないときにビジュアルを有効にする
				cameraImage.SetActive(isObserver && !isLocalPlayer);

				// トラッキングによる移動の有効を設定
				var to = GetComponent<TrackedObjects>();
				to.SetEnable((!isObserver && !forceUseKeyboardControl) || RtsTestNetworkManager.instance.ForceRelatedToTracking);
            }
            isObserverPrev = isObserver;
        }

        // 自分の時だけ「YOU]アイコン表示
        youIcon.SetActive(isLocalPlayer);

        // アイテムをつかんでいる時はつかんでいる位置にマイフレーム設定
        if (holdItem)
        {
            holdItem.transform.position = holdPos.position;
 //           holdItem.GetComponent<NetworkIdentity>().AssignClientAuthority( connectionToClient );
//            CmdUpdateHoldItemPosition();
        }

        if (isClient)
        {
        //    if (myDrothy == null)
        //    {
        //        var obj = ClientScene.FindLocalObject(drothyNetId);
        //        if( obj )
        //        {
        //            myDrothy = obj.GetComponent<DrothyController>();
        //        }
        //    }

            // サーバーの値が同期されるdrothyScaleの値でドロシーのスケールを更新
            if (myDrothy != null)
            {
                myDrothy.transform.localScale = Vector3.one * drothyScale;
            }
        }

        /////////////////////////////////////////
        // ■ここから↓はローカルプレイヤーのみ■
        /////////////////////////////////////////

        if ( !isLocalPlayer )
		{
			return;
		}

        if (forceUseKeyboardControl)
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

        if( forceUseKeyboardControl && Input.GetKeyDown( KeyCode.Space ) )
        {

        }

        if ( holdTarget && !holdItem && forceUseKeyboardControl && Input.GetKeyDown(KeyCode.H))
        {
            CmdSetHoldItem();
        }

        if (holdItem && forceUseKeyboardControl && Input.GetKeyDown(KeyCode.Y))
        {
            CmdEatItem();
        }

        if ( holdTarget && Vector3.Distance( transform.position, holdTarget.transform.position ) > 5f)
        {
            holdTarget = null;
            CmdReleaseHoldTarget();
        }

        // キーボードでOを押すと観測者になる(仮）
        if (Input.GetKeyDown(KeyCode.O))
        {
            //    photonView.RPC("SetObserver", PhotonTargets.AllBuffered, !_isObserver);
            //    photonView.RPC("SetObserver", PhotonTargets.AllBuffered, !_isObserver);
        }
        
    }

    /// <summary>
    /// ドロシーを生成する
    /// </summary>
    [Command]
    private void CmdCreateDrothy()
    //   private void CmdCreateDrothy( int colorIdx=1 ) // カラバリ対応用
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        // プレハブから生成
        var drothyIK = Instantiate<IKControl>(drothyIKPrefab);

        // IKのターゲットをトラッキングオブジェクトから取得
        {
            var trackedObj = GetComponent<TrackedObjects>();
            if( trackedObj != null )
            {
                drothyIK.rightHandObj = trackedObj.RightHandObject;
                drothyIK.leftHandObj = trackedObj.LeftHandObject;
                drothyIK.bodyObj = trackedObj.BodyObject;
                drothyIK.lookObj = trackedObj.LookTarget;
            }
        }

        // 設定が有効になっている場合は脚の動きのシミュレートを行う
        if (useSimulateFoot)
        {
            drothyIK.SetSimulateFoot();
        }

        // TODO プレイヤーIDによってカラバリを変更する
    //    drothyIK.GetComponent<DrothyController>().SetDressColor(colorIdx);

        myDrothy = drothyIK.GetComponent<DrothyController>();
        if( myDrothy == null )
        {
            return;
        }

        myDrothy.SetOwner(this.transform);

        // どちらが正しいかはまだ不明
//      NetworkServer.Spawn(drothy.gameObject);
        NetworkServer.SpawnWithClientAuthority(myDrothy.gameObject, gameObject);

    //    drothyNetId = myDrothy.netId;

        RpcPassDrothyReference(myDrothy.netId);
    }

    /// <summary>
    /// クライアント側でもドロシーの参照を持たせる
    /// </summary>
    [ClientRpc]
    private void RpcPassDrothyReference( NetworkInstanceId netId )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var obj = ClientScene.FindLocalObject(netId);

        myDrothy = obj.GetComponent<DrothyController>();

        // まだクライアント側はIKターゲットが未指定なのでセットする
        {
            var trackedObj = GetComponent<TrackedObjects>();
            var drothyIK = myDrothy.GetComponent<IKControl>();
            if (trackedObj != null && drothyIK != null)
            {
                drothyIK.rightHandObj = trackedObj.RightHandObject;
                drothyIK.leftHandObj = trackedObj.LeftHandObject;
                drothyIK.bodyObj = trackedObj.BodyObject;
                drothyIK.lookObj = trackedObj.LookTarget;
            }
        }

        // 仮 自分のドロシーは無効にする
        if (isLocalPlayer && !forceDisplayDrothy)
        {
            obj.SetActive(false);
        }
    }

    /// <summary>
    /// ドロシーへの参照を要求する
    /// </summary>
    [Command]
    private void CmdRequestDrothyReference()
    {
        if (myDrothy == null) return;
        RpcPassDrothyReference(myDrothy.netId);
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
    /// ローカルでのみ衝突を判定する
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if ((isObserver) || !isLocalPlayer ) return;

        if (other.tag.Equals("Item"))
        {
            var mush = other.GetComponent<Mushroom>();
            if (mush != null)
            {
                holdTarget = mush;
                CmdSetHoldTarget(mush.netId);
            } 
        }
    }

    /// <summary>
    /// つかみ候補のアイテムのセットをサーバーに反映する
    /// </summary>
    [Command]
    private void CmdSetHoldTarget( NetworkInstanceId id )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var obj = NetworkServer.FindLocalObject(id);
        if (!obj) return;
        var mush = obj.GetComponent<Mushroom>();
        if( mush != null )
        {
            holdTarget = mush;
        }
    }

    /// <summary>
    /// つかみ候補の解放をサーバーに反映する
    /// </summary>
    [Command]
    private void CmdReleaseHoldTarget()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        holdTarget = null;
    }

    /// <summary>
    /// つかんでいるアイテムをサーバーにも反映する
    /// </summary>
    [Command]
    private void CmdSetHoldItem()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
        if (!holdTarget) return;

        holdItem = holdTarget.GetComponent<Mushroom>();

        holdTarget = null;

        // つかんでいるプレイヤーに権限を与える
        var nIdentity = holdItem.GetComponent<NetworkIdentity>();
        if (nIdentity != null && !nIdentity.hasAuthority)
        {
            nIdentity.AssignClientAuthority(connectionToClient);
        }

        RpcSetHoldItem();
    }

    /// <summary>
    /// つかんでいるアイテムのセットをクライアントに反映する
    /// </summary>
    [ClientRpc]
    private void RpcSetHoldItem()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
        if (!holdTarget) return;

        holdItem = holdTarget.GetComponent<Mushroom>();
        holdTarget = null;
    }

    /// <summary>
    /// アイテムを消費する
    /// </summary>
    [Command]
    private void CmdEatItem( )
    {
        if (!holdItem) return;

        NetworkServer.Destroy(holdItem.gameObject);
        eatCount++;

        if( eatCount >= 3 && !biggenFlag )
        {
            biggenFlag = true;
            ChangeScale();
        }
    }

    [Server]
    private void ChangeScale()
    {
        drothyScale = 10f;
    }
}