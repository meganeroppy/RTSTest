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

    [ServerCallback]
    private void Start()
    {
        originPosition = transform.position;
    }

    private void Update()
    {
        visual.SetActive(enable);

        if( isServer )
            UpdateTimer();
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
    }

    [Server]
    private void UpdateTimer()
    {
        if (enable) return;

        respawnTimer -= Time.deltaTime;
        if( respawnTimer <= 0 )
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
    }
}
