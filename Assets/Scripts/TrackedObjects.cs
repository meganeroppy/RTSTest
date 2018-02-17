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
	private CopyTransform copyTransformHead = null;

    /// <summary>
    /// copyTransformHeadの子要素
    /// ドロシーモデルの頭が見つめる目標になる
    /// </summary>
	[SerializeField]
	private Transform lookTarget = null;
    public Transform LookTarget
    {
        get { return lookTarget; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、Touchの右手をトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformRightHand = null;

    /// <summary>
    /// ドロシーの右手のIK対象として位置角度を調整したもの。
    /// copyTransformRightHandの子要素
    /// </summary>
	[SerializeField]
	private Transform rightHandObject = null;
    public Transform RightHandObject
    {
        get { return rightHandObject; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、Touchの左手をトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformLeftHand = null;

    /// <summary>
    /// ドロシーの右手のIK対象として位置角度を調整したもの。
    /// copyTransformLeftHandの子要素
    /// </summary>
	[SerializeField]
	private Transform leftHandObject = null;
    public Transform LeftHandObject
    {
        get { return leftHandObject; }
    }



    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、サンダル左をトラッキングしたもの
    /// </summary>
    [SerializeField]
    private CopyTransform copyTransformLeftFoot = null;

    /// <summary>
    /// ドロシーの左足のIK対象として位置角度を調整したもの
    /// </summary>
	[SerializeField]
    private Transform leftFootObject = null;
    public Transform LeftFootObject
    {
        get { return leftFootObject; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、サンダル右をトラッキングしたもの
    /// </summary>
    [SerializeField]
    private CopyTransform copyTransformRightFoot = null;

    /// <summary>
    /// ドロシーの右足のIK対象として位置角度を調整したもの
    /// </summary>
	[SerializeField]
    private Transform rightFootObject = null;
    public Transform RightFootObject
    {
        get { return rightFootObject; }
    }

    /// <summary>
    /// シリアライズされたトラッキング位置と回転がそのまま適用されるオブジェクト、バックパックPCをトラッキングしたもの
    /// </summary>
    [SerializeField]
	private CopyTransform copyTransformBody = null;

    /// <summary>
    /// ドロシーの体のIK対象として位置角度を調整したもの。
    /// copyTransformBodyの子要素
    /// </summary>
    [SerializeField]
	private Transform bodyObject = null;
    public Transform BodyObject
    {
        get { return bodyObject; }
    }

    /// <summary>
    /// RTSの頭リジッドボディのY回転がこの値の時にHMDのRecenterを行うとベストな回転になる
    /// プレイヤーが生まれた時点で自動的に正面の値をセットできればベストだが、現状「今の角度が正面！」という設定しかできなそう（軽く調べたが情報なかった）なので
    /// 苦肉の策として頭リジッドボディのY回転がこの値に近くなったタイミングでRecenterをコールすることにする
    /// 美しい方法ではないので追々改善したい
    /// </summary>
    [SerializeField]
    private float bestTrackerHeadRotationY = 90f;

    /// <summary>
    /// RTSの頭リジッドボディY回転とベスト角度（↑）との差がこの値以下になったらそこを正面として定義する
    /// </summary>
    [SerializeField]
    private float centeringThreshold = 0.1f;

    private bool defineHmdCenter = false;

	/// <summary>
	/// CopyTransformのついた子要素の位置を0にした上で無効にする
	/// </summary>
	public void SetEnable(bool key)
	{
		var children = GetComponentsInChildren<CopyTransform>();
		foreach( CopyTransform c in children )
		{
			c.enabled = key;
			c.transform.localPosition = c.name.Contains("Head") ? Vector3.up * 1f : Vector3.zero;
		}
	}

	private void Start () 
	{
        if (!isLocalPlayer)
        {
			SetEnable(false);

			return;
		}

        var b = BaseSceneManager.instance;

        if (b != null)
        {
            copyTransformHead.copySource = b.CopyTransformHead.gameObject;
            copyTransformHead.offsetObject = b.OffsetObject;

            copyTransformRightHand.copySource = b.CopyTransformRightHand.gameObject;
            copyTransformRightHand.offsetObject = b.OffsetObject;

            copyTransformLeftHand.copySource = b.CopyTransformLeftHand.gameObject;
            copyTransformLeftHand.offsetObject = b.OffsetObject;

            copyTransformRightFoot.copySource = b.CopyTransformRightFoot.gameObject;
            copyTransformRightFoot.offsetObject = b.OffsetObject;

            copyTransformLeftFoot.copySource = b.CopyTransformLeftFoot.gameObject;
            copyTransformLeftFoot.offsetObject = b.OffsetObject;

            copyTransformBody.copySource = b.CopyTransformBody.gameObject;
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

        if( !defineHmdCenter )
        {
            if( copyTransformHead.copySource != null )
            {
                if( Mathf.Abs( copyTransformHead.copySource.transform.rotation.eulerAngles.y - bestTrackerHeadRotationY ) < centeringThreshold )
                {
                    OVRManager.display.RecenterPose();
                    defineHmdCenter = true;
                }
            }
        }
    }
}
