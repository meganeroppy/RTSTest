using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ティールームのシーン制御
/// </summary>
public class TeaRoomSceneManager : MonoBehaviour
{
    /// <summary>
    /// 巨大化ケーキのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject largenCakePrefab;

    /// <summary>
    /// 縮小化ケーキのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject smallenCakePrefab;

    /// <summary>
    /// キノコのプレハブ
    /// </summary>
    [SerializeField]
    private GameObject mushroomPrefab;

    /// <summary>
    /// 巨大化ケーキの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform[] largenCakePositions;

    /// <summary>
    /// 縮小化ケーキの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform[] smallenCakePositions;

    /// <summary>
    /// キノコの出現位置候補
    /// </summary>
    [SerializeField]
    private Transform[] mushroomPositions;

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

    public Transform[] GetLargenCakePositions()
    {
        return largenCakePositions;
    }

    public Transform[] GetSmallenCakePositions()
    {
        return smallenCakePositions;
    }


    public Transform[] GetMushroomPositions()
    {
        return mushroomPositions;
    }

}
