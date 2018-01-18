using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

/// <summary>
/// イベントの進行を管理
/// 基本的に観測者からの命令で動作する
/// </summary>
public class EventManager : NetworkBehaviour
{
    /// <summary>
    /// 自身へのインスタンス
    /// </summary>
    public static EventManager instance;

    [SerializeField]
    private GameObject mushroomPrefab;

    private enum Sequence
    {
        WaitRoom,
        CollapseGround_Event,
        Falling,
        TeaRoom,
        PopCakes1_Event,
        SmallenDrothy_Event,
        PopCakes2_Event,
        LargenDrothy_Event,
        PopMushrooms_Event,
        Ending_Event,
        Count_,
    }

    private int currentSequence = 0;

    [SerializeField]
    private string[] sceneNameList;

    /// <summary>
    /// 現在のシーンインデックス
    /// </summary>
    [SyncVar]
    private int currentSceneIndex = 0;

    /// <summary>
    /// 複数回使用するためメンバで持つ
    /// </summary>
    private TeaRoomSceneManager teaRoomSceneManager;
    private bool enableSmallenDrothy = false;

    private void Awake()
    {
        instance = this;
    }

    [Command]
    public void CmdProceedSequence()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        currentSequence = (currentSequence + 1) % (int)Sequence.Count_;

        var nextSequence = (Sequence)currentSequence;
        StartCoroutine(TransitionScene(nextSequence));
    }

    [SerializeField]
    private string baseSceneName = "StartScene";

    private IEnumerator TransitionScene(Sequence newSequence)
    {
        if (!newSequence.ToString().Contains("Event"))
        {
            // シーンの切り替え
            var newSceneName = newSequence.ToString();

            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogWarning(newSequence.ToString() + " シーンは存在しない ");
                yield break;
            }

            // TODO: 暗転演出

            // 必要なオブジェクトの所属シーンを引っ越し
            {
                var baseScene = SceneManager.GetSceneByName(baseSceneName);

                //		for( int i=0 ; i < TrackedObjects.list.Count ; ++i )
                //		{
                //			SceneManager.MoveGameObjectToScene( TrackedObjects.list[i].gameObject, baseScene );
                //		}

                var drothyTeam = GameObject.FindGameObjectsWithTag("Drothy");
                foreach (GameObject d in drothyTeam)
                {
                    SceneManager.MoveGameObjectToScene(d, baseScene);
                }
            }

            // もともとのシーンをアンロード
            {
                var scene = SceneManager.GetActiveScene();

                // アンロード実行
                var operation = SceneManager.UnloadSceneAsync(scene.name);

                // アンロードが終わるまで待機
                while (!operation.isDone) yield return null;
            }

            // 次のシーンをロードし、アクティブシーンにする
            {
                var operation = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);

                // ロードが終わるまで待機
                while (!operation.isDone) yield return null;

                var scene = SceneManager.GetSceneByName(newSceneName);
                SceneManager.SetActiveScene(scene);
            }

            // TODO: 暗転解除
        }
        else
        {
            // シーン切り替えなし
            ExecEvent(newSequence);
        }
    }


    /// <summary>
    /// イベントの実行
    /// </summary>
	private void ExecEvent(Sequence newEvent)
    {
        Debug.Log("イベント " + newEvent.ToString() + "の実行");

        GameObject obj = null;

        switch (newEvent)
        {
            case Sequence.CollapseGround_Event:

                // 地面か崩れるイベント
                obj = GameObject.Find("GardenSceneManager");
                if (!obj) break;

                obj.SendMessage("PlayEvent");

                break;
            case Sequence.PopCakes1_Event:

                // 縮小ケーキが出現するイベント
                obj = GameObject.Find("TeaRoomSceneManager");
                if (!obj) break;

                teaRoomSceneManager = obj.GetComponent<TeaRoomSceneManager>();

                // 生成位置を統一するために自分自身の場合のみ位置を選定し、リモートに共有する

                if (isLocalPlayer)
                //                if( !photonView.isMine )
                {
                    Debug.Log("自分自身でないためケーキ生成位置の選定を行わない");
                    return;
                }

                // 出現候補を取得
                var positions = teaRoomSceneManager.GetSmallenCakePositions();

                // プレイヤー数を取得 自身は除く
                int observerNum = 1; // TODO 観測者が複数いるとたぶにバグになる
                int playerNum = observerNum;
                //       int playerNum = TrackedObjects.list.Count - observerNum;
                List<int> indexs = new List<int>();

                // プレイヤー数まで位置を選定
                while (indexs.Count < playerNum)
                {
                    int key = Random.Range(0, positions.Length);
                    if (!indexs.Contains(key))
                    {
                        indexs.Add(key);
                    }
                }

                //        photonView.RPC("SetLargenCakes", PhotonTargets.All, indexs.ToArray());
                SetSmallenCakes(indexs.ToArray());

                break;
            case Sequence.SmallenDrothy_Event:
                // ドロシーが小さくなるイベントのフラグを立てる
                enableSmallenDrothy = true;

                break;
            case Sequence.PopCakes2_Event:
                //      var managerObj = GameObject.Find("GardenSceneManager");
                //      if (!managerObj) break;

                //      var manager = managerObj.GetComponent<CollapseFloor>();
                //      if (manager == null) break;

                //      manager.PlayEvent();

                break;
            case Sequence.LargenDrothy_Event:
                //     var managerObj = GameObject.Find("GardenSceneManager");
                //      if (!managerObj) break;
                //
                //      var manager = managerObj.GetComponent<CollapseFloor>();
                //      if (manager == null) break;

                //      manager.PlayEvent();

                break;
            case Sequence.PopMushrooms_Event:
                //    var managerObj = GameObject.Find("GardenSceneManager");
                //    if (!managerObj) break;

                //    var manager = managerObj.GetComponent<CollapseFloor>();
                //    if (manager == null) break;

                //    manager.PlayEvent();

                break;
            case Sequence.Ending_Event:
                //    var managerObj = GameObject.Find("GardenSceneManager");
                //    if (!managerObj) break;

                //    var manager = managerObj.GetComponent<CollapseFloor>();
                //    if (manager == null) break;

                //   manager.PlayEvent();

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// きのこ配置
    /// </summary>
    [Command]
    public void CmdCreateMushroom()
    {
        var obj = Instantiate(mushroomPrefab);
        obj.transform.position = transform.position;
        var mush = obj.GetComponent<Mushroom>();
        mush.CmdSetParent(this.gameObject);

        NetworkServer.Spawn(obj);
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    [Command]
    public void CmdGotoNextScene()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        currentSceneIndex++;
        RpcGotoNextScene(currentSceneIndex, true);
    }

    /// <summary>
    /// 次のシーンに移動する
    /// </summary>
    /// <param name="newSceneIndex"></param>
    /// <param name="allowLoadSameScene"></param>
    [ClientRpc]
    private void RpcGotoNextScene(int newSceneIndex, bool allowLoadSameScene)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        if (currentSceneIndex != newSceneIndex || allowLoadSameScene)
        {
            currentSceneIndex = newSceneIndex;

            SceneManager.LoadScene(sceneNameList[currentSceneIndex % sceneNameList.Length], LoadSceneMode.Additive);

            if (currentSceneIndex >= 1)
            {
                SceneManager.UnloadSceneAsync(sceneNameList[(currentSceneIndex - 1) % sceneNameList.Length]);
            }
        }
    }

    //   [PunRPC]
    private void SetLargenCakes(int[] indexs)
    {
        foreach (int idx in indexs)
        {
            teaRoomSceneManager.SetLargenCakes(idx);
        }
    }

    //    [PunRPC]
    private void SetSmallenCakes(int[] indexs)
    {
        foreach (int idx in indexs)
        {
            teaRoomSceneManager.SetSmallenCakes(idx);
        }
    }

}
