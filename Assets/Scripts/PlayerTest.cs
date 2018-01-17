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
	private Rigidbody myRigidbody;
    public float moveSpeed = 10f;
    public float rotSpeed = 10f;
    private NetworkView netView = null;
	private NetworkTransform nTransform = null;

    /// <summary>
    /// 管理者か？
    /// </summary>
	[SyncVar]
	private bool isObserver = false;
    private bool isObserverPrev = false;

    /// <summary>
    /// 脚の動きのシミュレーションを有効にするか？
    /// </summary>
    public bool useSimulateFoot = false;

    [SerializeField]
	private MeshRenderer observerSign;

    [SerializeField]
    private DrothySample drothyPrefabOld;
    private DrothySample drothyOld;

    [SerializeField]
    private IKControl drothyIKPrefab;
    private IKControl myDrothy;

    [SerializeField]
    private GameObject bulletPrefab;

    [SerializeField]
    private GameObject mushroomPrefab;

    [SerializeField]
    private TextMesh textMesh;

    [SerializeField]
    private GameObject youIcon;

    [SyncVar]
    private string netIdStr;

    [SerializeField]
    private Transform holdPos;

    [SyncVar]
    private int eatCount = 0;

    [SyncVar]
    private float drothyScale = 1f;

    [SyncVar]
    private int currentSceneIndex = 0;

    [SerializeField]
    private bool forceBehaveLikePlayer = false;

    [SyncVar]
    private NetworkInstanceId drothyNetId;

    /// <summary>
    /// （仮）プリセットのプレイヤーカラー
    /// </summary>
    Color[] playerColor = new Color[] { Color.gray, Color.red, Color.green, Color.blue, Color.yellow };

    /// <summary>
    /// キーボード操作を有効にするか？
    /// </summary>
    public bool useKeyboardControl = false;

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


    [SerializeField]
    private string[] sceneNameList;

    private void Awake()
    {
        Debug.Log("Awake" + "isObserver = " + isObserver.ToString() + " local= " + isLocalPlayer.ToString());
    }

    // Use this for initialization
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        netView = GetComponent<NetworkView>();
        nTransform = GetComponent<NetworkTransform>();

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

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer" + "isObserver = " + isObserver.ToString() + " local= " + isLocalPlayer.ToString());

        CmdCreateDrothy();

        CmdSetNetIdStr();

        CmdSetIsObserver(NetworkManagerTest.instance.IsObserver);

        if(NetworkManagerTest.instance.IsObserver)
        {
            var obj = GameObject.Find("BaseSceneManager");
            if( obj )
            {
                var manager = obj.GetComponent<BaseSceneManager>();
                if( manager != null )
                {
                    manager.ActivatePresetCameras();
                }
            }
        }
    }

    [Command]
    private void CmdSetNetIdStr()
    {
        netIdStr = netId.Value.ToString();
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient" + "isObserver = " + isObserver.ToString() + " local= " + isLocalPlayer.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        // 自分自身でなくともラベルは全て自分を向く
        if (playerLabel.enabled && Camera.main != null)
        {
            playerLabel.transform.forward = Camera.main.transform.forward;
        }

        textMesh.text = netIdStr;

        if (isObserver != isObserverPrev)
        {
            observerSign.material.color = isObserver ? Color.red : Color.white;
        }

        isObserverPrev = isObserver;

        youIcon.SetActive(isLocalPlayer);

        if (holdItem)
        {
            holdItem.transform.position = holdPos.position;
 //           holdItem.GetComponent<NetworkIdentity>().AssignClientAuthority( connectionToClient );
//            CmdUpdateHoldItemPosition();
        }

        if (isClient)
        {
            if (drothyOld == null)
            {
                var obj = ClientScene.FindLocalObject(drothyNetId);
                if( obj )
                {
                    drothyOld = obj.GetComponent<DrothySample>();
                }
            }

            if (drothyOld != null)
            {
                drothyOld.transform.localScale = Vector3.one * drothyScale;
            }
        }

        // ■ここから↓はローカルプレイヤーのみ■

        if ( !nTransform.isLocalPlayer )
		{
			return;
		}

		Vector3 move = new Vector3(
			Input.GetAxisRaw("Horizontal"),
            0,
			Input.GetAxisRaw("Vertical"));

        transform.Translate( move * moveSpeed * Time.deltaTime);
        //	myRigidbody.velocity = (transform.forward + move.normalized) * fSpeed ;

        var rot = Input.GetKey(KeyCode.I) ? -1 : Input.GetKey(KeyCode.O) ? 1 : 0;
        if( rot != 0 )
        {
            transform.Rotate(Vector3.up * rot * rotSpeed * Time.deltaTime);
        }

        if( Input.GetKeyDown( KeyCode.Space ) )
        {
            CmdFire();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isObserver)
        {
            CmdCreateMushroom();
        }

        if (Input.GetKeyDown(KeyCode.N) && isObserver)
        {
            CmdGotoNextScene();
        }

        if ( holdTarget && !holdItem && Input.GetKeyDown(KeyCode.H))
        {
            CmdSetHoldItem();
        }

        if (holdItem && Input.GetKeyDown(KeyCode.Y))
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


        if (useSimulateFoot)
        {
            drothyIK.SetSimulateFoot();
        }

        // TODO プレイヤーIDによってカラバリを変更する
    //    drothyIK.GetComponent<DrothyController>().SetDressColor(colorIdx);

        myDrothy = drothyIK;

    
        drothyOld.SetOwner(this.transform);

//        NetworkServer.Spawn(drothy.gameObject);
        NetworkServer.SpawnWithClientAuthority(drothyOld.gameObject, gameObject);

        drothyNetId = drothyOld.netId;

        RpcPassDrothyReference(drothyOld.netId);
    }

    /// <summary>
    /// クライアント側でもドロシーの参照を持たせる
    /// </summary>
    [ClientRpc]
    private void RpcPassDrothyReference( NetworkInstanceId netId )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var obj = ClientScene.FindLocalObject(netId);

        drothyOld = obj.GetComponent<DrothySample>();
    }

    /// <summary>
    /// ドロシーへの参照を要求する
    /// </summary>
    [Command]
    private void CmdRequestDrothyReference()
    {
        if (drothyOld == null) return;
        RpcPassDrothyReference(drothyOld.netId);
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
    /// （仮）銃弾を発射する
    /// </summary>
    [Command]
    private void CmdFire()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        var obj = Instantiate(bulletPrefab);
        obj.transform.position = transform.position;
        obj.GetComponent<Rigidbody>().AddForce(transform.forward * 80f);
        obj.GetComponent<MeshRenderer>().material.color = isObserver ? Color.red : Color.white;

        NetworkServer.Spawn(obj);
    }

    /// <summary>
    /// きのこ配置
    /// </summary>
    [Command]
    private void CmdCreateMushroom()
    {
        var obj = Instantiate(mushroomPrefab);
        obj.transform.position = transform.position;
        var mush = obj.GetComponent<Mushroom>();
        mush.CmdSetParent(this.gameObject);

        NetworkServer.Spawn(obj);
    }

    /// <summary>
    /// ローカルでのみ衝突を判定する
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        if ((isObserver && !forceBehaveLikePlayer) || !isLocalPlayer ) return;

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

    [SyncVar]
    private bool biggenFlag = false;

    [Server]
    private void ChangeScale()
    {
        drothyScale = 10f;
    }

    /// <summary>
    /// アイテムの位置を更新する
    /// </summary>
    [Command]
    private void CmdUpdateHoldItemPosition()
    {
        holdItem.CmdSetPosition(holdPos.position);
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    [Command]
    private void CmdGotoNextScene()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        currentSceneIndex++;
        RpcGotoNextScene(currentSceneIndex, true);
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    /// <param name="newSceneIndex"></param>
    /// <param name="allowLoadSameScene"></param>
    [ClientRpc]
    private void RpcGotoNextScene( int newSceneIndex, bool allowLoadSameScene )
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        if (currentSceneIndex != newSceneIndex || allowLoadSameScene)
        {
            currentSceneIndex = newSceneIndex;

            SceneManager.LoadScene(sceneNameList[currentSceneIndex % sceneNameList.Length], LoadSceneMode.Additive);

            if (currentSceneIndex >= 1)
            {
            SceneManager.UnloadSceneAsync(sceneNameList[(currentSceneIndex - 1) % sceneNameList.Length]);
            }
        }
    }
}