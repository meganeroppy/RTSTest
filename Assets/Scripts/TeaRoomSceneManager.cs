﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ティールームのシーン制御
/// 色々準備してるけど全部イベントマネージャでやったほうが楽かも
/// 場合によってはこちらのメソッドたちも使用する
/// とりあえず現状はアイテムの生成トランスフォームを保持するのみ？
/// アイテム生成でなくシーンそのものに変化（スケール変更など）を起こす際に使う予定
/// </summary>
public class TeaRoomSceneManager : MonoBehaviour
{
    public static TeaRoomSceneManager instance;

    /// <summary>
    /// ケーキを置く場所の親要素
    /// </summary>
    [SerializeField]
    private Transform cakeSetPositionParent = null;
    public Transform CakeSetPositionParent { get{ return cakeSetPositionParent;} }
    private Transform[] cakeSetPositions;
    public Transform[] CakeSetPositions { get { return cakeSetPositions; } }

    /// <summary>
    /// 巨大化ケーキの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform largenCakePositionsParent = null;
    private Transform[] largenCakePositions;
    public Transform[] LargenCakeTrans { get { return largenCakePositions; } }

    /// <summary>
    /// 縮小化ケーキの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform smallenCakePositionsParent = null;
    private Transform[] smallenCakePositions;
    public Transform[] SmallenCakeTrans { get { return smallenCakePositions; } }

    /// <summary>
    /// キノコの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform mushroomPositionsParent = null;
    private Transform[] mushroomPositions;
    public Transform[] MushroomTrans { get { return mushroomPositions; } }

    private void Start()
    {
        instance = this;

        largenCakePositions = new Transform[largenCakePositionsParent.childCount];
        for( int i=0; i < largenCakePositionsParent.childCount; ++i )
        {
            largenCakePositions[i] = largenCakePositionsParent.GetChild(i);
        }

        smallenCakePositions = new Transform[smallenCakePositionsParent.childCount];
        for (int i = 0; i < smallenCakePositionsParent.childCount; ++i)
        {
            smallenCakePositions[i] = smallenCakePositionsParent.GetChild(i);
        }

        mushroomPositions = new Transform[mushroomPositionsParent.childCount];
        for (int i = 0; i < mushroomPositionsParent.childCount; ++i)
        {
            mushroomPositions[i] = mushroomPositionsParent.GetChild(i);
        }

        cakeSetPositions = new Transform[cakeSetPositionParent.childCount];
        for (int i = 0; i < cakeSetPositionParent.childCount; ++i)
        {
            cakeSetPositions[i] = cakeSetPositionParent.GetChild(i);
        }

    }

    /// <summary>
    /// プレイヤーを大きくなる演出（※プレイヤー以外の環境オブジェクトを小さくして表現する
    /// </summary>
    public void LargenPlayers()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// プレイヤーを小さくする演出（※プレイヤー以外の環境オブジェクトを大きくして表現する）
    /// </summary>
    public void SmallenPlayers()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// 小さくなるケーキを出現させる
    /// </summary>
    public void SetSmallenCakes(int idx)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

    //    PhotonNetwork.Instantiate(smallenCakePrefab.name, smallenCakePositions[idx].position, Quaternion.identity, 0);
    }

    /// <summary>
    /// 大きくなるケーキを出現させる
    /// </summary>
    public void SetLargenCakes(int idx)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

    //    PhotonNetwork.Instantiate(smallenCakePrefab.name, largenCakePositions[idx].position, Quaternion.identity, 0);
    }

    /// <summary>
    /// キノコをを出現させる
    /// </summary>
    public void SetMushrooms()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());
    }
}
