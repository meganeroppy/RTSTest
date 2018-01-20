using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/// <summary>
/// トラッキング関連の処理を行う
/// </summary>
public class TrackedObjects : NetworkBehaviour
{
    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、HMDをトラッキングしたもの
    /// </summary>
	[SerializeField]
	private CopyTransform copyTransformHead;

    /// <summary>
    /// copyTransformHeadの子要素
    /// ドロシーモデルの頭が見つめる目標になる
    /// </summary>
	[SerializeField]
	private Transform lookTarget;
    public Transform LookTarget
    {
        get { return lookTarget; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、Touchの右手をトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformRightHand;

    /// <summary>
    /// ドロシーの右手のIK対象として位置角度を調整したもの。
    /// copyTransformRightHandの子要素
    /// </summary>
	[SerializeField]
	private Transform rightHandObject;
    public Transform RightHandObject
    {
        get { return rightHandObject; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、Touchの左手をトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformLeftHand;

    /// <summary>
    /// ドロシーの右手のIK対象として位置角度を調整したもの。
    /// copyTransformLeftHandの子要素
    /// </summary>
	[SerializeField]
	private Transform leftHandObject;
    public Transform LeftHandObject
    {
        get { return leftHandObject; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、バックパックPCをトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformBody;

    /// <summary>
    /// ドロシーの体のIK対象として位置角度を調整したもの。
    /// copyTransformBodyの子要素
    /// </summary>
    [SerializeField]
	private Transform bodyObject;
    public Transform BodyObject
    {
        get { return bodyObject; }
    }

	[SerializeField]
	private Transform rotateCopyFrom;

	[SerializeField]
	private Transform rotateCopyTo;

	public bool forceDisableCopyTransform = false;

	/// <summary>
	/// Initialize & SetObserver より後に呼ばれる前提であるため注意する
	/// </summary>
	void Start () 
	{
        //	Debug.Log( gameObject.name + " " + System.Reflection.MethodBase.GetCurrentMethod() + "mine=" + (photonView != null && photonView.isMine).ToString() ) ;

        if (!isLocalPlayer || forceDisableCopyTransform)
        {
            var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}
		}

        if (!isLocalPlayer)
        {
            this.copyTransformHead.enabled = false;
            this.copyTransformRightHand.enabled = false;
            this.copyTransformLeftHand.enabled = false;
            this.copyTransformBody.enabled = false;

            return;
        }

        var b = BaseSceneManager.instance;

        if (b != null)
        {
            copyTransformHead.copySource = b.CopyTransformHead;
            copyTransformHead.offsetObject = b.OffsetObject;

            copyTransformRightHand.copySource = b.CopyTransformRightHand;
            copyTransformRightHand.offsetObject = b.OffsetObject;

            copyTransformLeftHand.copySource = b.CopyTransformLeftHand;
            copyTransformLeftHand.offsetObject = b.OffsetObject;

            copyTransformBody.copySource = b.CopyTransformBody;
            copyTransformBody.offsetObject = b.OffsetObject;
        }
    }

    // Update is called once per frame
    void Update () 
	{		
        // 自身でなければなにもしない
        if ( !isLocalPlayer ) return;

        // とりあえず仮でRを押して頭の回転リセット
        // TODO この操作をしなくても自動で頭の回転が調整できるようにしたい
        if ( Input.GetKeyDown(KeyCode.R) )
		{
			OVRManager.display.RecenterPose();
		}
    }

	/// <summary>
	/// モデルの回転 自分自身の時はあまり意味がないが、人から見られるときに重要になる
	/// </summary>
	public void UpdateVisualRotation()
	{
		if( rotateCopyFrom != null && rotateCopyTo != null )
		{
			rotateCopyTo.forward = rotateCopyFrom.forward;
		}
	}
}
