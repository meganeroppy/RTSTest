using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// アイテム
/// 主にドロシーが食べる
/// </summary>
public class DrothyItem : NetworkBehaviour
{
    [SyncVar]
    private bool enable = true;

    /// <summary>
    /// 見た目
    /// </summary>
    [SerializeField]
    private GameObject visual;

    /// <summary>
    /// 再出現タイマー
    /// </summary>
    private float respawnTimer = 0;

    /// <summary>
    /// 再出現までの時間
    /// </summary>
    [SerializeField]
    private float respawnWait = 5f;

    /// <summary>
    /// 最初に生まれた場所
    /// 食べられてから一定時間たつとここに再配置される
    /// </summary>
    private Vector3 originPosition;

    [SerializeField]
    private float effectTime = 2f;
    public float EffectTime { get { return effectTime; } }

    private AudioSource audioSource;

    /// <summary>
    /// 出現時SE
    /// </summary>
    [SerializeField]
    private AudioClip popSound;

    /// <summary>
    /// 食べられた時のSE
    /// </summary>
    [SerializeField]
    private AudioClip eatenSound;

    /// <summary>
    /// つかまれたときのSE
    /// </summary>
    [SerializeField]
    private AudioClip heldSound;

    /// <summary>
    /// すでにほかの人がつかんでいるアイテムを奪ったりできないようにフラグを作る
    /// </summary>
    [SyncVar]
    private bool holdable = true;
    public bool Holdable { get { return holdable; } }

    [ServerCallback]
    private void Start()
    {
        originPosition = transform.position;

        // SE再生
        RpcPlaySpawnSound();

        holdable = true;
    }

    private void Update()
    {
        // ビジュアルの有効をセット enableはSyncVar
        visual.SetActive(enable);

        // サーバー側のみタイマーを更新する
        if (isServer)
            UpdateTimer();
    }

    /// <summary>
    /// つかまれる
    /// </summary>
    [Server]
    public void SetHeld()
    {
        holdable = false;

        RpcPlayHeldSound();
    }

    /// <summary>
    /// 食べられる
    /// </summary>
    [Command]
    public void CmdEaten()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        enable = false;

        respawnTimer = respawnWait;

        RpcPlayEatenSound();
    }

    [Server]
    private void UpdateTimer()
    {
        if (enable) return;

        respawnTimer -= Time.deltaTime;
        if (respawnTimer <= 0)
        {
            Respawn();
        }
    }

    /// <summary>
    /// 再出現
    /// </summary>
    [Server]
    private void Respawn()
    {
        transform.position = originPosition;

        enable = true;
        holdable = true;

        // SE再生
        RpcPlaySpawnSound();
    }

    /// <summary>
    /// 食べられた時のSE再生
    /// </summary>
    [ClientRpc]
    private void RpcPlayEatenSound()
    {
        // 本人しか聞こえない?
//        if (!isLocalPlayer) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.PlayOneShot(eatenSound);
    }

    /// <summary>
    /// 出現したときのSEを再生
    /// </summary>    
    [ClientRpc]
    public void RpcPlaySpawnSound()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.PlayOneShot(popSound);
    }

    /// <summary>
    /// つかまれたときのSEを再生
    /// </summary>    
    [ClientRpc]
    private void RpcPlayHeldSound()
    {
        // 本人しか聞こえない?
 //       if (!isLocalPlayer) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        audioSource.PlayOneShot(heldSound);
    }
}

