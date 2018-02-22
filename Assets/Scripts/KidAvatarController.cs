using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// こどもアバターの制御
/// </summary>
public class KidAvatarController : AvatarController
{
	private List<GameObject> kidsList;

	private Transform objectRoot;

	public int testIndex = 0;
    protected override void Init()
    {
        //仮
        if (isServer)
        {
        //    SetActiveKid(testIndex);
        }
    }

    private void SetKidsList()
    {
        kidsList = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            kidsList.Add(transform.GetChild(i).gameObject);
        }
    }

    [Server]
	public void SetActiveKidServer( int index )
	{
        SetKidsList();

        // 指定インデックス以外の要素を削除
        var kid = kidsList[index];
        kid.gameObject.SetActive(true);
        kid.transform.SetSiblingIndex(0);

        index %= kidsList.Count;
        for (int i = kidsList.Count - 1; i >= 0; --i)
        {
            if (i != index)
            {
                kidsList[i].SetActive(false);
            }
        }

        RpcSetActiveKid(index);
	}

    [ClientRpc]
    private void RpcSetActiveKid( int index )
    {
        SetKidsList();

        SetActiveKid(index);
    }

    private void SetActiveKid( int index )
    {
        // 指定インデックス以外の要素を削除
        var kid = kidsList[index];
        kid.gameObject.SetActive(true);
        kid.transform.SetSiblingIndex(0);

        index %= kidsList.Count;
        for (int i = kidsList.Count - 1; i >= 0; --i)
        {
            if (i != index)
            {
                kidsList[i].SetActive(false);
            }
        }
        objectRoot = kid.transform;

        modelRoot = transform;

        anim = kid.GetComponent<Animator>();

        StartCoroutine(Reactive());
    }

    /// <summary>
    /// 一旦自身を無効にして次のフレームで有効にする
    /// </summary>
    private IEnumerator Reactive()
    {
        gameObject.SetActive(false);

        yield return null;

        gameObject.SetActive(true);
    }
}
