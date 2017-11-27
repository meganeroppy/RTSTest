﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : Photon.PunBehaviour
{	
	/// <summary>
	/// 重複しないようにする
	/// </summary>
	[SerializeField]
	private byte playerId = 0;

	[SerializeField]
	private TrackerSettings[] trackerSettings;

	public GameObject prefab;

	[SerializeField]
	private string StartSceneName;

	[SerializeField]
	private GameObject copyTransformHead;

	[SerializeField]
	private GameObject copyTransformRightHand;

	[SerializeField]
	private GameObject copyTransformLeftHand;

	[SerializeField]
	private GameObject offsetObject;

	void Awake()
	{
		// 指定されたシーンをロード
		SceneManager.LoadSceneAsync( StartSceneName, LoadSceneMode.Additive );
	}

	void Start()
    {
	//	foreach( TrackerSettings t in trackerSettings ) 
	//	{
	//		t.ObjectName
	//	}

        // 指定の設定でPhotonネットワークに接続
        PhotonNetwork.ConnectUsingSettings("1.0.0");
        PhotonNetwork.sendRate = 30;
    }

    // 「ロビー」に接続した際に呼ばれるコールバック
    public override void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby");
        PhotonNetwork.JoinRandomRoom();
    }

    // いずれかの「ルーム」への接続に失敗した際のコールバック
    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed");

        // ルームを作成（今回の実装では、失敗＝マスタークライアントなし、として「ルーム」を作成）
        PhotonNetwork.CreateRoom(null);
    }

    // Photonサーバに接続した際のコールバック
    public override void OnConnectedToPhoton()
    {
        Debug.Log("OnConnectedToPhoton");
    }

    // マスタークライアントに接続した際のコールバック
    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        PhotonNetwork.JoinRandomRoom();
    }

    // いずれかの「ルーム」に接続した際のコールバック
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");

        // 「ルーム」に接続したらPrefabを生成する（動作確認用）
		var obj = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity, 0);
		InitializeTrackedObjects( obj );
    }

	private void InitializeTrackedObjects( GameObject obj )
	{
		var trackedObjects = obj.GetComponent<TrackedObjects>();
		if( trackedObjects == null )
		{
			Debug.LogError("trackedObject is null");
			return;
		}

		trackedObjects.Initialize(copyTransformHead, copyTransformRightHand, copyTransformLeftHand, offsetObject, playerId);
	}

	/// <summary>
	/// Photonネットワークへの接続失敗時のコールバック
	/// </summary>
	public void OnFailedToConnectToPhoton()
	{
		var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
		InitializeTrackedObjects( obj ); 
	}

    // 現在の接続状況を表示（デバッグ目的）
    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
}