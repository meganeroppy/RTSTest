using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 参加しているプレイヤーの数だけ出現する
/// 一定ダメージを受けると死亡状態になる
/// 出現しているすべての個体が死亡状態になるとシナリオ進行フラグが立つ
/// 死亡状態になってから一定時間が経過すると復活する（＝ある程度同時に死亡状態にする必要がある）
/// </summary>
public class ShootTarget : NetworkBehaviour
{
    [SyncVar]
    private bool activated = true;
    public bool Activated { get { return activated; } }

    private const int maxHealth = 3;

    private int currentHealth;

    private float reviveTime = 5f;

    private float timer = 0;

    /// <summary>
    /// 見た目
    /// </summary>
    [SerializeField]
    private GameObject visual;

	// Use this for initialization
    [ServerCallback]
	void Start ()
    {
        activated = true;
        currentHealth = maxHealth;	
	}
	
	// Update is called once per frame
    [ServerCallback]
	void Update () {

        if (Input.GetKeyDown(KeyCode.H)) CmdHit();

        if (activated) return;

        timer += Time.deltaTime;
        if( timer >= reviveTime )
        {
            Revive();
        }
	}

    /// <summary>
    /// 攻撃がヒット
    /// </summary>
    [Command]
    public void CmdHit()
    {
        Hit();
    }

    [Server]
    private void Hit()
    {
        if (!activated) return;

        currentHealth--;
        if (currentHealth <= 0)
        {
            Dead();
        }
    }

    [Server]
    private void Dead()
    {
        activated = false;
        timer = 0;
    }

    /// <summary>
    /// 復活
    /// </summary>
    [Server]
    private void Revive()
    {
        activated = true;
        currentHealth = maxHealth;
    }
}
