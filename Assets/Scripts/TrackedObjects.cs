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
	[SerializeField]
	private CopyTransform copyTransformHead;

	[SerializeField]
	private Transform lookTarget;
    public Transform LookTarget
    {
        get { return lookTarget; }
    }

	[SerializeField]
	private CopyTransform copyTransformRightHand;

	[SerializeField]
	private Transform rightHandObject;
    public Transform RightHandObject
    {
        get { return rightHandObject; }
    }

    [SerializeField]
	private CopyTransform copyTransformLeftHand;

	[SerializeField]
	private Transform leftHandObject;
    public Transform LeftHandObject
    {
        get { return leftHandObject; }
    }

    [SerializeField]
	private CopyTransform copyTransformBody;

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

        // 自身でない時の処理
        if (!isLocalPlayer)
        {
            // CopyTransform削除
            var children = GetComponentsInChildren<CopyTransform>();
			foreach( CopyTransform c in children )
			{
				c.enabled = false;
			}

		}

		if( forceDisableCopyTransform )
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

            if (isLocalPlayer)
            {
                //        photonView.RPC( "SetObserver", PhotonTargets.AllBuffered, !_isObserver );
            }
            else
            {
                SetObserver(true);
            }
            return;
        }

        BaseSceneManager b = null;
        b = BaseSceneManager.instance;

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
        if ( !isLocalPlayer ) return;

        // とりあえず仮でRを押して頭の回転リセット
        // TODO この操作をしなくても自動で頭の回転が調整できるようにしたい
        if ( Input.GetKeyDown(KeyCode.R) )
		{
			OVRManager.display.RecenterPose();
		}
    }

	/// <summary>
	/// 観測者に指定
	/// </summary>
	//[PunRPC]   
	public void SetObserver( bool value )
	{
		_isObserver = value;

		var children = GetComponentsInChildren<CopyTransform>();
		foreach( CopyTransform c in children )
		{
			c.enabled = !_isObserver;
		}

		copyTransformRightHand.gameObject.SetActive( !_isObserver );
		copyTransformLeftHand.gameObject.SetActive( !_isObserver );

		if( _isObserver )
		{
			copyTransformHead.transform.localPosition = Vector3.zero;
		}
	}		
	private bool _isObserver;


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
