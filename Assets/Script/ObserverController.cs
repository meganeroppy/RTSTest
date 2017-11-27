﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverController : Photon.MonoBehaviour {

	[SerializeField]
	private float move_speed = 2f;

	[SerializeField]
	private float rot_speed = 2f;

	// Use this for initialization
	void Start () 
	{
		cameraRotate = transform.localRotation;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( photonView.ownerId != 0 && !photonView.isMine ) return;

		UpdatePosition();
		UpdateRotaition();
		UpdateKeyInput();
	}

	void UpdatePosition()
	{
		var move_x = Input.GetAxis("Horizontal");
		var move_z = Input.GetAxis("Vertical");

		var move_y = 0;
		if( Input.GetKey( KeyCode.Space ) )
		{
			move_y++;
		}
		if( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
		{
			move_y--;
		}

		transform.position += transform.right * move_x * move_speed * Time.deltaTime;
		transform.position += transform.forward * move_z * move_speed * Time.deltaTime;
		transform.position += transform.up * move_y * move_speed * Time.deltaTime;


		//var add = new Vector3( move_x, move_y, move_z);

		//transform.position += add * move_speed * Time.deltaTime;
	}

	Quaternion cameraRotate;

	void UpdateRotaition()
	{
		var xRotate = Input.GetAxis("Mouse Y") * rot_speed;
		var yRotate = Input.GetAxis("Mouse X") * rot_speed;

		//　マウスを上に移動した時に上を向かせたいなら反対方向に角度を反転させる
		xRotate *= -1;

		cameraRotate *= Quaternion.Euler(xRotate, yRotate, 0f);

		//　カメラの視点変更を実行
		transform.localRotation = Quaternion.Slerp(transform.localRotation, cameraRotate, rot_speed * Time.deltaTime);
	}

	void UpdateKeyInput()
	{
		// 視点の傾きをリセット
		if( Input.GetKeyDown(KeyCode.R) )
		{
			var newRot = Quaternion.Euler( 0, transform.localRotation.eulerAngles.y, 0 );
			cameraRotate = newRot;
		}

		// Tはテスト用のキー
		if( Input.GetKeyDown(KeyCode.T) )
		{
			//	photonView.RPC("ExecTest", PhotonTargets.AllBuffered);
			ExecTest();
		}
	}

	[PunRPC]
	private void ExecTest()
	{
		if( photonView.ownerId != 0 )
		{
			PhotonNetwork.Instantiate("TestHead", transform.position, transform.rotation, 0);
		}
	}
}
