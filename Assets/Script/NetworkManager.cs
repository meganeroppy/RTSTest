using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : Photon.PunBehaviour
{
    public string ObjectName;
    public string masterServerAddress = "127.0.0.1";

    void Start()
    {
        // Photonネットワークの設定を行う
        PhotonNetwork.ConnectToMaster(masterServerAddress, 5055, "rtstest", "1.0.0");
//        PhotonNetwork.ConnectUsingSettings("0.1");
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

        // 「ルーム」に接続したらCubeを生成する（動作確認用）
        GameObject cube = PhotonNetwork.Instantiate(ObjectName, Vector3.zero, Quaternion.identity, 0);
    }

    // 現在の接続状況を表示（デバッグ目的）
    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }
}