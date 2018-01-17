using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class RtsNetworkManager : NetworkBehaviour
{
    /// <summary>
    /// 重複しないようにする
    /// </summary>
    [SerializeField]
    private byte playerId = 0;

    public GameObject prefab;

    [SerializeField]
    private string StartSceneName;

    [SerializeField]
    private TrackerSettings copyTransformHead;
    public GameObject CopyTransformHead { get { return copyTransformHead.gameObject; } }

	[SerializeField]
	private TrackerSettings copyTransformRightHand;
    public GameObject CopyTransformRightHand { get { return copyTransformRightHand.gameObject; } }

    [SerializeField]
	private TrackerSettings copyTransformLeftHand;
    public GameObject CopyTransformLeftHand { get { return copyTransformLeftHand.gameObject; } }

    [SerializeField]
	private TrackerSettings copyTransformBody;
    public GameObject CopyTransformBody { get { return copyTransformBody.gameObject; } }

    [SerializeField]
	private GameObject offsetObject;
    public GameObject OffsetObject { get { return offsetObject.gameObject; } }

    IEnumerator Start()
    {
		// 文字列指定があれば指定されたシーンをロード
		if( !string.IsNullOrEmpty( StartSceneName ) )
		{
			SceneManager.LoadSceneAsync( StartSceneName, LoadSceneMode.Additive );

			var scene = SceneManager.GetSceneByName( StartSceneName );
			while ( !scene.isLoaded )
			{
				yield return null;
			}
			// アクティブなシーンに設定
			SceneManager.SetActiveScene( scene );
		}

		// プレイヤーIDが0でない時はトラッカーセッティングにオブジェクト名をセット
		if( playerId != 0 )
		{
			if( copyTransformHead.ObjectName.EndsWith("Head") )
				copyTransformHead.ObjectName = copyTransformHead.ObjectName + playerId.ToString();
		
			if( copyTransformRightHand.ObjectName.EndsWith("RH") )				
				copyTransformRightHand.ObjectName = copyTransformRightHand.ObjectName + playerId.ToString();
	
			if( copyTransformLeftHand.ObjectName.EndsWith("LH") )				
				copyTransformLeftHand.ObjectName = copyTransformLeftHand.ObjectName + playerId.ToString();

			if( copyTransformBody.ObjectName.EndsWith("Body") )				
				copyTransformBody.ObjectName = copyTransformBody.ObjectName + playerId.ToString();
		}
    }

	/// <summary>
	/// トラックされるオブジェクトを初期化
	/// </summary>
	private void InitializeTrackedObjects( GameObject obj )
	{
		var trackedObjects = obj.GetComponent<TrackedObjects>();
		if( trackedObjects == null )
		{
			Debug.LogError("trackedObject is null");
			return;
		}

	//	trackedObjects.Initialize(copyTransformHead.gameObject, copyTransformRightHand.gameObject, copyTransformLeftHand.gameObject, copyTransformBody.gameObject, offsetObject, playerId);
	} 
}