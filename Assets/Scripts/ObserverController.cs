using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

/// <summary>
/// 観測者クラス
/// イベントマネージャにコマンドを送ることでシナリオを進行させる役割もある
/// </summary>
public class ObserverController : NetworkBehaviour {

	[SerializeField]
	private float move_speed = 2f;

	[SerializeField]
	private float rot_speed = 2f;

    Quaternion cameraRotate;

    [SerializeField]
    GameObject navigateShotPrefab;

	private TrackedObjects trackedObjects;

    // Use this for initialization
    void Start () 
	{
		cameraRotate = transform.localRotation;
		trackedObjects = GetComponent<TrackedObjects>();
	}

    // Update is called once per frame
    void Update()
    {
        // ローカルの自分自身でなかったらなにもしない
        if (!isLocalPlayer) return;

		// 位置と回転を更新
		UpdatePosition();
        UpdateRotaition();
        CheckInput();
	}

	void UpdatePosition()
	{
		// 参加型観測者の時はなにもしない
		if (RtsTestNetworkManager.instance.MyObserverType.Equals( RtsTestNetworkManager.ObserverType.Participatory )) return;

		// 強制トラッカー依存操作の時はなにもしない
		if (RtsTestNetworkManager.instance.MyInputMode.Equals( RtsTestNetworkManager.InputMode.ForceByTracking )) return;

        var move_x = Input.GetAxis("Horizontal");
        var move_z = Input.GetAxis("Vertical");

        var move_y = 0;
        if (Input.GetKey(KeyCode.Space))
        {
            move_y++;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            move_y--;
        }

        transform.position += transform.right * move_x * move_speed * Time.deltaTime;
        transform.position += transform.forward * move_z * move_speed * Time.deltaTime;
            transform.position += transform.up * move_y * move_speed * Time.deltaTime;
	}

    void UpdateRotaition()
    {
		// 参加型観測者の時はなにもしない
		if (RtsTestNetworkManager.instance.MyObserverType.Equals( RtsTestNetworkManager.ObserverType.Participatory )) return;

		// 強制トラッカー依存操作の時はなにもしない
		if (RtsTestNetworkManager.instance.MyInputMode.Equals( RtsTestNetworkManager.InputMode.ForceByTracking )) return;

        var xRotate = Input.GetAxis("Mouse Y") * rot_speed;
        var yRotate = Input.GetAxis("Mouse X") * rot_speed;

        //　マウスを上に移動した時に上を向かせたいなら反対方向に角度を反転させる
        xRotate *= -1;

        cameraRotate *= Quaternion.Euler(xRotate, yRotate, 0f);

        //　カメラの視点変更を実行
        transform.localRotation = Quaternion.Slerp(transform.localRotation, cameraRotate, rot_speed * Time.deltaTime);

        // 視点の傾きをリセット
        if (Input.GetKeyDown(KeyCode.R))
        {
            var newRot = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
            cameraRotate = newRot;
        }			
    }		

	bool pullingRightIndexTrigger = false;
	bool pullingLeftIndexTrigger = false;
	bool pullingRightHandTrigger = false;
	bool pullingLeftHandTrigger = false;

	/// <summary>
	/// 入力管理
	/// </summary>
    private void CheckInput()
    {
        // これを呼ばないとOVRInputのメソッドが動かないらしいので呼ぶ
        OVRInput.Update();

        // ナビゲートショット発射
		{
	        if (
				Input.GetButtonDown("Fire1")// マウス左クリック
			)
	        {
	            FireNavigateShotWithMouse();
	        }

			var pullRight = OVRInput.Get (OVRInput.RawAxis1D.RIndexTrigger) > 0;

			if( pullRight && !pullingRightIndexTrigger )
			{
				pullingRightIndexTrigger = true;

				// 右手から発射
				FireNavigateShot(true);
			}
			else if( !pullRight )
			{
				pullingRightIndexTrigger = false;
			}

			var pullLeft = OVRInput.Get (OVRInput.RawAxis1D.LIndexTrigger) > 0;

			if( pullLeft && !pullingLeftIndexTrigger )
			{
				pullingLeftIndexTrigger = true;

				// 左手から発射
				FireNavigateShot(false);
			}
			else if( !pullLeft )
			{
				pullingLeftIndexTrigger = false;
			}
		}

		// イベントを進める
		{
			if( Input.GetKeyDown(KeyCode.P) )// キーボードのP
			{
				CmdProceesSequence();
			}

			var pullRight = OVRInput.Get (OVRInput.RawAxis1D.RHandTrigger) > 0;

			if( pullRight & !pullingRightHandTrigger )
			{
				pullingRightHandTrigger = true;
				CmdProceesSequence();
			}
			else if( !pullRight )
			{
				pullingRightHandTrigger = false;
			}

			var pullLeft = OVRInput.Get (OVRInput.RawAxis1D.LHandTrigger) > 0;

			if( pullLeft & !pullingLeftHandTrigger )
			{
				pullingLeftHandTrigger = true;
				CmdProceesSequence();
			}
			else if( !pullLeft )
			{
				pullingLeftHandTrigger = false;
			}				
		}
    }

    [Command]
    private void CmdProceesSequence()
    {
        var em = EventManager.instance;
        if (em == null) return;
		em.ForceProceedSequence();
    }

	/// <summary>
	/// 道筋やヒントをプレイヤーに与えるためのショットを発射
	/// キーボード操作版
	/// </summary>
	[Client]
	private void FireNavigateShotWithMouse()
	{
		if( Camera.main == null ) return;
		
		var clickPosition = Input.mousePosition;
		// Z軸修正
		clickPosition.z = 10f;
		var targetPos = Camera.main.ScreenToWorldPoint(clickPosition);

		// 生成はサーバー側で行う
		CmdFireNavigateShotWithMouse(targetPos);
	}

	/// <summary>
	/// 道筋やヒントをプレイヤーに与えるためのショットを発射
	/// Touch操作版
	/// </summary>
	[Client]
	private void FireNavigateShot( bool right )
	{
		var obj = right ? trackedObjects.RightHandObject : trackedObjects.LeftHandObject;
		// 生成はサーバー側で行う
		CmdFireNavigateShot(obj);
	}

    [Command]
	private void CmdFireNavigateShotWithMouse( Vector3 targetPos )
	{
        var obj = Instantiate(navigateShotPrefab);
        obj.transform.position = transform.position;
        obj.transform.LookAt(targetPos);

        NetworkServer.Spawn(obj);
	}

	[Command]
	private void CmdFireNavigateShot( Transform muzzleTrans )
	{
		var obj = Instantiate(navigateShotPrefab);
		obj.transform.position = muzzleTrans.position;
		obj.transform.rotation = muzzleTrans.rotation;

		NetworkServer.Spawn(obj);
	}
}
