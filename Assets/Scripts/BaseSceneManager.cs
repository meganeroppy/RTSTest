using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseSceneManager : MonoBehaviour
{
    public static BaseSceneManager instance;

    public string firstSceneName;

    [SerializeField]
    private Camera[] presetCameras;

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

    // Use this for initialization
    void Start ()
    {
        instance = this;
        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);

        int playerId = 0;
        if (NetworkManagerTest.instance != null) playerId = NetworkManagerTest.instance.GetPlayerId();

        if (playerId != 0)
        {
            if (copyTransformHead.ObjectName.EndsWith("Head"))
                copyTransformHead.ObjectName = copyTransformHead.ObjectName + playerId.ToString();

            if (copyTransformRightHand.ObjectName.EndsWith("RH"))
                copyTransformRightHand.ObjectName = copyTransformRightHand.ObjectName + playerId.ToString();

            if (copyTransformLeftHand.ObjectName.EndsWith("LH"))
                copyTransformLeftHand.ObjectName = copyTransformLeftHand.ObjectName + playerId.ToString();

            if (copyTransformBody.ObjectName.EndsWith("Body"))
                copyTransformBody.ObjectName = copyTransformBody.ObjectName + playerId.ToString();
        }
    }

    public void ActivatePresetCameras()
    {
        for( int i = 0 ; i < presetCameras.Length; ++i )
        {
            presetCameras[i].enabled = true;
            presetCameras[i].targetDisplay = i+1;
        }
    }
}
