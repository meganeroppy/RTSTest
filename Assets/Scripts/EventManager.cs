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
        Garden,
        CollapseGround_Event,
        Falling,
        TeaRoom,
        PopCakes1_Event,
        SmallenDrothy_Event,
        TeaRoomLarge,
        PopCakes2_Event,
        LargenDrothy_Event,
        TeaRoomSmall,
        PopMushrooms_Event,
        Ending_Event,
        Count_,
    }

    private int currentSequence = 0;

    /// <summary>
    /// 削除候補
    /// </summary>
    [System.Obsolete]
    [SerializeField]
    private string[] sceneNameList;

    /// <summary>
    /// 現在のシーンインデックス
    /// </summary>
    [System.Obsolete]
    [SyncVar]
    private int currentSceneIndex = 0;

    private enum ItemType
    {
        SmallenCake,
        LargenCake,
        Mushroom,
    }

    /// <summary>
    /// このフラグが有効な時に各プレイヤーが同時にケーキを食べるとドロシーが小さくなる
    /// 論理値で管理しなくてもシーケンスをみて判断できそうなきもするので削除候補
    /// </summary>
    private bool enableSmallenDrothy = false;

    /// <summary>
    /// 削除候補
    /// シーン遷移時に生成済みオブジェクトの配置シーン移動を行う際に必要になる可能性がある
    /// </summary>
    [SerializeField]
    private string baseSceneName = "StartScene";

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

        {
            var baseScene = SceneManager.GetSceneByName(baseSceneName);

            var players = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject g in players)
            {
                SceneManager.MoveGameObjectToScene(g, baseScene);
            }

            var drothies = GameObject.FindGameObjectsWithTag("Drothy");
            foreach (GameObject d in drothies)
            {
                SceneManager.MoveGameObjectToScene(d, baseScene);
            }

        }

        // 各クライアントでイベントを進める
        RpcProceedSequence(nextSequence);
    }

    [ClientRpc]
    private void RpcProceedSequence(Sequence newSequence)
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        currentSequence = (int)newSequence;

        StartCoroutine(ExecSequence(newSequence));
    }

    private IEnumerator ExecSequence(Sequence newSequence)
    {
        if (!newSequence.ToString().Contains("Event"))
        {
            Debug.Log("シーン遷移 ->" + newSequence.ToString());

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

                var players = GameObject.FindGameObjectsWithTag("Player");

                foreach(  GameObject g in players )
                {
                	SceneManager.MoveGameObjectToScene( g, baseScene );
                }

                var drothies = GameObject.FindGameObjectsWithTag("Drothy");
                foreach (GameObject d in drothies)
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
    /// シーンの変更を伴わないイベントの実行
    /// </summary>
	private void ExecEvent(Sequence newEvent)
    {
        Debug.Log("イベント " + newEvent.ToString() + "の実行");

        switch (newEvent)
        {
            case Sequence.CollapseGround_Event:

                // 地面か崩れるイベント
                // やりかたかえてもいいかも？

                GameObject　obj = GameObject.Find("GardenSceneManager");
                if (!obj) break;

                obj.SendMessage("PlayEvent");

                break;
            case Sequence.PopCakes1_Event:

                // 縮小化ケーキが出現するイベント                
                CmdCreateItems( ItemType.SmallenCake );

                break;
            case Sequence.SmallenDrothy_Event:

                // ドロシー縮小化フラグを有効
                enableSmallenDrothy = true;

                break;
            case Sequence.PopCakes2_Event:

                // 巨大化ケーキ出現
                CmdCreateItems(ItemType.LargenCake);

                break;
            case Sequence.LargenDrothy_Event:

                // ドロシー巨大化フラグを有効

                break;
            case Sequence.PopMushrooms_Event:

                // きのこ出現
                CmdCreateItems(ItemType.Mushroom);

                break;
            case Sequence.Ending_Event:
            default:

                // エンディングから待機画面にもどる                
                break;
        }
    }

    /// <summary>
    /// きのこ配置
    /// </summary>
    [Command]
    private void CmdCreateItems( ItemType type )
    {
        if (TeaRoomSceneManager.instance == null) return;

        // 出現候補を取得
        var positions =
            type == ItemType.SmallenCake ? TeaRoomSceneManager.instance.SmallenCakePositions :
            type == ItemType.LargenCake ? TeaRoomSceneManager.instance.LargenCakePosition :            
            TeaRoomSceneManager.instance.MushroomPositions;

        // TODO プレイヤー数を取得
        int PlayerNum = 1; 

        var obj = Instantiate(mushroomPrefab);
        obj.transform.position = positions[Random.Range(0, positions.Length)].position;

        NetworkServer.Spawn(obj);
    }
}