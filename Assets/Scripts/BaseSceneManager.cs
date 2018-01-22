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
    private Transform copyTransformHead;
    public GameObject CopyTransformHead { get { return copyTransformHead.gameObject; } }

    [SerializeField]
    private Transform copyTransformRightHand;
    public GameObject CopyTransformRightHand { get { return copyTransformRightHand.gameObject; } }

    [SerializeField]
    private Transform copyTransformLeftHand;
    public GameObject CopyTransformLeftHand { get { return copyTransformLeftHand.gameObject; } }

    [SerializeField]
    private Transform copyTransformBody;
    public GameObject CopyTransformBody { get { return copyTransformBody.gameObject; } }

    [SerializeField]
    private GameObject offsetObject;
    public GameObject OffsetObject { get { return offsetObject.gameObject; } }

	private bool serverOnly = false;

    // Use this for initialization
    void Start ()
    {
        instance = this;

        StartCoroutine(LoadFirstScene());

        int playerId = 0;
        if (RtsTestNetworkManager.instance != null) playerId = RtsTestNetworkManager.instance.GetPlayerId();

        if (playerId != 0)
        {
            if (copyTransformHead.gameObject.name.EndsWith("Head"))
                copyTransformHead.gameObject.name = copyTransformHead.gameObject.name + playerId.ToString();

            if (copyTransformRightHand.gameObject.name.EndsWith("RH"))
                copyTransformRightHand.gameObject.name = copyTransformRightHand.gameObject.name + playerId.ToString();

            if (copyTransformLeftHand.gameObject.name.EndsWith("LH"))
                copyTransformLeftHand.gameObject.name = copyTransformLeftHand.gameObject.name + playerId.ToString();

            if (copyTransformBody.gameObject.name.EndsWith("Body"))
                copyTransformBody.gameObject.name = copyTransformBody.gameObject.name + playerId.ToString();
        }

		if( RtsTestNetworkManager.instance.GetRole() == RtsTestNetworkManager.Role.Server )
		{
			serverOnly = true;
			ActivatePresetCameras();
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
