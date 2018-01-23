using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ベースとなるシーンを制御
/// </summary>
public class BaseSceneManager : MonoBehaviour
{
    public static BaseSceneManager instance;

    public string firstSceneName;

    [SerializeField]
    private Camera[] presetCameras;

    [SerializeField]
    private TrackerSettings copyTransformHead;
    public TrackerSettings CopyTransformHead { get { return copyTransformHead; } }

    [SerializeField]
    private TrackerSettings copyTransformRightHand;
    public TrackerSettings CopyTransformRightHand { get { return copyTransformRightHand; } }

    [SerializeField]
    private TrackerSettings copyTransformLeftHand;
    public TrackerSettings CopyTransformLeftHand { get { return copyTransformLeftHand; } }

    [SerializeField]
    private TrackerSettings copyTransformBody;
    public TrackerSettings CopyTransformBody { get { return copyTransformBody; } }

    [SerializeField]
    private GameObject offsetObject;
    public GameObject OffsetObject { get { return offsetObject.gameObject; } }

	private bool serverOnly = false;

    // Use this for initialization
    void Start()
    {
        instance = this;

        StartCoroutine(LoadFirstScene());

        if (RtsTestNetworkManager.instance.GetRole() == RtsTestNetworkManager.Role.Server)
        {
            serverOnly = true;
            ActivatePresetCameras();
        }

        // RTSモーションシミュレーションフラグがあったら設定する
        if (RtsTestNetworkManager.instance.SimulateRtsMovement)
        {
            var obj = GetComponent<RtsMovementSample>();
            if (obj != null) obj.Init();
        }

        int playerId = 0;
        if (RtsTestNetworkManager.instance != null) playerId = RtsTestNetworkManager.instance.GetPlayerId();

        // プレイヤーIDに従って参照するRTSのリジットボディ名を指定する
        {
            if (copyTransformHead.ObjectName.EndsWith("Head"))
                copyTransformHead.ObjectName += playerId.ToString();

            if (copyTransformRightHand.ObjectName.EndsWith("RH"))
                copyTransformRightHand.ObjectName += playerId.ToString();

            if (copyTransformLeftHand.ObjectName.EndsWith("LH"))
                copyTransformLeftHand.ObjectName += playerId.ToString();

            if (copyTransformBody.ObjectName.EndsWith("Body"))
                copyTransformBody.ObjectName += playerId.ToString();
        }
    }
    private IEnumerator LoadFirstScene()
    {
        var operation = SceneManager.LoadSceneAsync(firstSceneName, LoadSceneMode.Additive);

        while (!operation.isDone) yield return null;

        var scene = SceneManager.GetSceneByName(firstSceneName);

        SceneManager.SetActiveScene( scene );

    }

    public void ActivatePresetCameras()
    {
		// サーバーのみの場合はDisplay1から、ホストの時は自身の視界がDisplay1なのでDisplay2から
		int diff = serverOnly ? 0 : 1; 

        for( int i = 0 ; i < presetCameras.Length; ++i )
        {
            presetCameras[i].enabled = true;
			presetCameras[i].targetDisplay = i+diff;
        }
    }
}
