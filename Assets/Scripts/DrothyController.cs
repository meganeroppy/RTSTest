using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DrothyController : NetworkBehaviour 
{
	private Animator anim;
	private bool walking;

	[SerializeField]
	private float threasholdMoveSpeed = 1f;

	[SerializeField]
	private Transform modelRoot;

	private Vector3 prevPosition;

	[SerializeField]
	private SkinnedMeshRenderer skinMesh;

	private int dressMatIdx = 1;

	[SerializeField]
	private Texture[] dressTexture;

    private Transform owner = null;

    [SyncVar]
    private Vector3 ownerPosition;

    [SyncVar]
    private Quaternion ownerRotation;

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

    /// <summary>
    /// サーバーからのみ呼ぶこと
    /// </summary>
    public void SetOwner(Transform owner)
    {
        this.owner = owner;
        initialized = true;
    }

    // Use this for initialization
    void Start ()
	{
		anim = GetComponent<Animator>();
		prevPosition = modelRoot.localPosition;
		walking = false;

        Debug.Log("DrothyController isLocal=" + isLocalPlayer.ToString());
	}

	// Update is called once per frame
	void Update () 
	{
		var vDiff = (prevPosition - modelRoot.localPosition);
//		Debug.Log( vDiff );

		var diff = vDiff.magnitude;

//			Debug.Log( diff.ToString() );

		walking = diff >= threasholdMoveSpeed;

		if( walking != anim.GetBool("Walking") )
		{
			anim.SetBool("Walking", walking);
		}
		
		prevPosition = modelRoot.localPosition;

		if( falling )
		{			
			if( enableLoop && Mathf.Abs( transform.position.y ) > loopThresholdHeight )
			{
				transform.position = new Vector3( transform.position.x, originHeight, transform.position.z);
			}

			var speedDif = fallingSpeedMax - fallingSpeedMin;
			if( speedDif == 0 ) speedDif = 1;

			var speed = fallingSpeedMin + Mathf.PingPong(Time.time * interval, speedDif);
			transform.position += Vector3.down * speed * Time.deltaTime;
        }

        UpdateOwnerPositionAndRotation();

        transform.SetPositionAndRotation(ownerPosition, ownerRotation);

		SetDressColor();

        UpdateDeleteFlag();

        if (deleteFlag)
        {
            Destroy(gameObject);
        }

        // まばたき
        UpdateBlinking();

        // 口ぱく
        UpdateTalk();
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
		var newTex = dressTexture[ colorIdx % dressTexture.Length];
		if( skinMesh.materials[ dressMatIdx ].mainTexture != newTex )
		{
			skinMesh.materials[ dressMatIdx ].mainTexture = newTex;
		}
	}

	private bool falling = false;
	public float fallingSpeedMax = 1f;
	public float fallingSpeedMin = 1f;
	public float interval = 1f;
	public bool enableLoop = false;
	public void SetIsFalling()
	{
		SetIsFalling(!falling);
	}
	public void SetIsFalling(bool value)
	{
		falling = value;
	}

	private float originHeight = 0;
	public float loopThresholdHeight = 20f;
	public void StartFalling()
	{
		originHeight = transform.position.y;
		SetIsFalling( true );
	}

    /// <summary>
    ///  まばたき更新
    /// </summary>
    private void UpdateBlinking()
    {
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
        if (talking > 0) talking -= Time.deltaTime;
        anim.SetBool("MouseOpen", talking > 0);
    }

    [Command]
    public void CmdTalk()
    {
        talking = 1f;
    }
}
