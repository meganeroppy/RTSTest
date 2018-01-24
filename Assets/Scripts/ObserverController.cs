﻿using System.Collections;
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

    // Use this for initialization
    void Start () 
	{
		cameraRotate = transform.localRotation;
	}

    // Update is called once per frame
    void Update()
    {
        // ローカルの自分自身でなかったらなにもしない
        if (!isLocalPlayer) return;

    //    if (!RtsTestNetworkManager.instance.ForceRelatedToTracking)
    //    {
            UpdatePosition();
            UpdateRotaition();
    //    }
        UpdateEvent();
        CheckKeyInput();
	}

	void UpdatePosition()
	{
        if (!RtsTestNetworkManager.instance.ForceRelatedToTracking)
        {
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
		//var add = new Vector3( move_x, move_y, move_z);

		//transform.position += add * move_speed * Time.deltaTime;
	}

    void UpdateRotaition()
    {
        if (!RtsTestNetworkManager.instance.ForceRelatedToTracking)
        {
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
    }

	void UpdateEvent()
	{
        if(EventManager.instance == null )
        {
            return;
        }

        var em = EventManager.instance;

		// シーケンス開始
		if(!RtsTestNetworkManager.instance.ForceRelatedToTracking && Input.GetKeyDown(KeyCode.P) )
		{
            em.CmdProceedSequence();
		}
	}

    private void CheckKeyInput()
    {
        if( Input.GetButtonDown("Fire1") ||
            Input.GetButtonDown("Oculus_GearVR_RIndexTrigger") ||
            Input.GetButtonDown("Oculus_GearVR_RIndexTrigger") )
        {
            CmdFireNavigateShot();
        }
    }

    /// <summary>
    /// 道筋やヒントをプレイヤーに与えるためのショットを発射
    /// </summary>
    [Command]
    private void CmdFireNavigateShot()
	{
        var obj = Instantiate(navigateShotPrefab);
        obj.transform.position = transform.position;

        var clickPosition = Input.mousePosition;
        // Z軸修正
        clickPosition.z = 10f;
        var targetPos = Camera.main.ScreenToWorldPoint(clickPosition);

        obj.transform.LookAt(targetPos);

        NetworkServer.Spawn(obj);
	}
}
