using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarController : NetworkBehaviour 
{
	protected Animator anim;
	private bool walking;

	[SerializeField]
	private float threasholdMoveSpeed = 1f;

	[SerializeField]
	protected Transform modelRoot;

	protected Vector3 prevPosition;

	[SerializeField]
	private SkinnedMeshRenderer skinMesh;

	private int dressMatIdx = 1;

	[SerializeField]
	private Texture[] dressTexture;

    private Transform owner = null;

    [SyncVar]
	protected Vector3 ownerPosition;

    [SyncVar]
	protected Quaternion ownerRotation;

    [SyncVar]
    private Vector3 targetPosition;

    [SyncVar]
    private bool deleteFlag = false;

	[SyncVar]
	private int colorIdx;
	public int ColorIdx { get{ return colorIdx; } set{ colorIdx = value; }}

    /// <summary>
    /// プレイヤーが発言しているか？
    /// </summary>
    [SyncVar]
    private float talking = 0;

    private bool initialized = false;

    /// <summary>
    /// まばたき頻度
    /// </summary>
    private float blinkingInterval = 5f;

    /// <summary>
    /// まばたきタイマー
    /// </summary>
    private float blinkingTimer = 0;

	[SerializeField]
	private bool useWalkAnim = false;

    /// <summary>
    /// サーバーからのみ呼ぶこと
    /// </summary>
    public void SetOwner(Transform owner)
    {
        this.owner = owner;
        initialized = true;
    }

    // Use this for initialization
    void Awake ()
	{
		Init();
		walking = false;

        Debug.Log("AvatarController isLocal=" + isLocalPlayer.ToString());
	}

	protected virtual void Init()
	{
		anim = GetComponent<Animator>();
		if( useWalkAnim )
		{
			prevPosition = modelRoot.localPosition;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		// 一定の速度以上で移動していたら歩きアニメにする
		if( useWalkAnim )
		{
			var vDiff = (prevPosition - modelRoot.localPosition);
	//		Debug.Log( vDiff );

			var diff = vDiff.magnitude;
	//		Debug.Log( diff.ToString() );

			walking = diff >= threasholdMoveSpeed;

			if( walking != anim.GetBool("Walking") )
			{
				anim.SetBool("Walking", walking);
			}
			
			prevPosition = modelRoot.localPosition;
		}

		// 位置と回転の更新
		{
			// サーバー側のみオーナーの位置を参照してメンバ変数ownerPosition、ownerRotationに代入する
			if( isServer )
			{
	        	UpdateOwnerPositionAndRotation();
			}

			// サーバーとクライアント双方で位置を更新 ownerPositionとownerRotationはSyncVarなのでサーバーの値がクライアントにも同期される
			UpdatePositionAndRotation();
		}

		// ドレスの色変更 サーバーとクライアント両方
		SetDressColor();

		// 削除フラグ関連
		{
			// サーバー側でフラグの更新を行う
			if( isServer )
			{
	        	UpdateDeleteFlag();
			}

			// フラグによる処理をサーバーとクライアント両方で行う
	        if (deleteFlag)
	        {
	            Destroy(gameObject);
	        }
		}

		// まばたきと口パク
		{
	        // まばたき
	        UpdateBlinking();

	        // 口ぱく
	        UpdateTalk();
	    }
	}

    /// <summary>
    /// サーバーのみ
    /// オーナーの位置と回転をSyncVar付きの変数に設定する
    /// ＊直接ownerをSyncVarにすると文句言われる＊
    /// </summary>
    [Server]
    private void UpdateOwnerPositionAndRotation()
    {
        if (owner == null) return;

        ownerPosition = owner.position;
        ownerRotation = owner.rotation;
    }

	protected virtual void UpdatePositionAndRotation()
	{
		transform.SetPositionAndRotation(ownerPosition, ownerRotation);
	}

    /// <summary>
    /// サーバーのみ
    /// 初期化後にオーナーがいなくなったら削除フラグを立てる
    /// </summary>
    [Server]
    private void UpdateDeleteFlag()
    {
        deleteFlag = initialized && owner == null;
    }

	/// <summary>
	/// カラーインデックスに応じてドレス色を変更
	/// </summary>
	public void SetDressColor()
	{
        if (skinMesh == null) return;

		var newTex = dressTexture[ colorIdx % dressTexture.Length];
		if( skinMesh.materials[ dressMatIdx ].mainTexture != newTex )
		{
			skinMesh.materials[ dressMatIdx ].mainTexture = newTex;
		}
	}

    /// <summary>
    ///  まばたき更新
    /// </summary>
    private void UpdateBlinking()
    {
		if( anim == null ) return;

        blinkingTimer += Time.deltaTime;
        if (blinkingTimer >= blinkingInterval)
        {
            anim.SetTrigger("EyeClose");
            blinkingTimer = 0;
        }        
    }

    /// <summary>
    /// 口ぱく更新
    /// </summary>
    private void UpdateTalk()
    {
		if( anim == null ) return;
        if (talking > 0) talking -= Time.deltaTime;
        anim.SetBool("MouseOpen", talking > 0);
    }

    [Command]
    public void CmdTalk()
    {
        talking = 1f;
    }
}
