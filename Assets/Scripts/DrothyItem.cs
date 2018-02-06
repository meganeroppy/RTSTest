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
    private bool eatable = true;

    /// <summary>
    /// 見た目
    /// </summary>
    [SerializeField]
    private GameObject visual;

    /// <summary>
    /// 再出現タイマー
    /// </summary>
    private float resetTimer = 0;

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

	/// <summary>
	/// 最初に生まれた時の回転
	/// </summary>
	private Quaternion originRotation;

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

	/// <summary>
	/// 一度掴まれてから離されたか？
	/// 再出減したらfalseになる
	/// </summary>
	private bool released = false;

    [ServerCallback]
    private void Start()
    {
        originPosition = transform.position;
		originRotation = transform.rotation;

        // SE再生
        RpcPlaySpawnSound();

        holdable = true;
		released = false;
    }

    private void Update()
    {
        // ビジュアルの有効をセット enableはSyncVar
        visual.SetActive(eatable);

        // サーバー側のみタイマーを更新する
        if (isServer)
            UpdateTimer();
    }

    /// <summary>
    /// つかまれる
	/// falseの時は離される
    /// </summary>
    [Server]
	public void SetHeld( bool val )
    {
        holdable = !val;
		released = !val;

		if( val )
		{
        	RpcPlayHeldSound();
		}
		else
		{
			// 一度つかんでからはなしたアイテムは一定時間で初期位置にセット
			resetTimer = respawnWait;
		}
    }

    /// <summary>
    /// 食べられる
    /// </summary>
    [Command]
    public void CmdEaten()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		eatable = false;

        resetTimer = respawnWait;

        RpcPlayEatenSound();
    }

    [Server]
    private void UpdateTimer()
    {
		if (eatable && !released) return;

		resetTimer -= Time.deltaTime;
        if (resetTimer <= 0)
        {
            Reset();
        }
    }

    /// <summary>
	/// 初期位置にセット
    /// </summary>
    [Server]
    private void Reset()
    {
        transform.position = originPosition;
		transform.rotation = originRotation;

        eatable = true;
        holdable = true;
		released = false;

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