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
    private DrothyItem itemPrefab;

    /// <summary>
    /// いらないかも
    /// </summary>
    public static List<DrothyItem> itemList;

    /// <summary>
    /// シナリオの流れ
    /// </summary>
    public enum Sequence
    {
        Garden, // 一番初めのシーン
        CollapseGround_Event, // 床が崩れていく
        Falling, // 落下中のシーン
        TeaRoom, // ティールーム
        PopCakes1_Event, // ケーキが出現する
        SmallenDrothy_Event, // ドロシー達が小さくなる
        TeaRoomLarge, // ドロシーが小さくなったあとのティールーム
        PopCakes2_Event, // ケーキが出現する
        LargenDrothy_Event, // ドロシー達が巨大化する
        TeaRoomSmall, // 小さな（破壊された）ティールーム
        PopMushrooms_Event, // きのこが出現する
        Ending_Event, // 終了演出
        Count_,
    }

    private Sequence currentSequence = 0;
    public Sequence CurrentSequence { get { return currentSequence; } }

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

    private void Update()
    {
        CheckPlayerEffect();
    }

    /// <summary>
    /// 参加しているプレイヤーのアイテム使用状況をチェックする
    /// </summary>
    [Server]
    private void CheckPlayerEffect()
    {
        // ふさわしいシーケンスでなければなにもしない
        if (currentSequence != Sequence.PopCakes1_Event && currentSequence != Sequence.PopCakes2_Event && currentSequence != Sequence.PopMushrooms_Event) return;

        // プレイヤーリストがなければなにもしない
        if (PlayerTest.list == null) return;

        // 全員アイテム効果中だったら次のシーンに移行
        bool allPlayersInEffect = true;
        foreach( PlayerTest p in PlayerTest.list )
        {
            bool inEffect = p.ItemEffectTimer > 0;


            Debug.Log( "ID" + p.netId.ToString() + " : " + ( inEffect ? "効果中" : "効果なし" ) );
            if (!inEffect) allPlayersInEffect = false;
        }

        if( allPlayersInEffect )
        {
            // 全員効果中なので次のシーケンスに移行
            Debug.Log("全員効果中なので次のシーケンスに移行");

            ProceedSequence();
        }
    }

    [Command]
    public void CmdProceedSequence()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        ProceedSequence();
    }

    private void ProceedSequence()
    {
        currentSequence = (Sequence)(((int)currentSequence + 1) % (int)Sequence.Count_);

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

        currentSequence = newSequence;

        StartCoroutine(ExecSequence(newSequence));
    }
    /// <summary>
    /// イベントの実行
    /// 場合によってはシーン切り替えを含む
    /// </summary>
	private IEnumerator ExecSequence(Sequence newSequence)
    {
        Debug.Log("イベント " + newSequence.ToString() + "の実行");

        switch (newSequence)
        {
            case Sequence.CollapseGround_Event:

                // 地面が崩れるイベント
                var manager = GardenSceneManager.instance;
                if (manager == null) break;

                manager.PlayEvent();

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

        // シーン切り替えを伴う場合は切り替え処理
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
    }

    /// <summary>
    /// きのこ配置
    /// </summary>
    [Command]
    private void CmdCreateItems( ItemType type )
    {
        if (TeaRoomSceneManager.instance == null) return;

        // 出現候補を取得
        var Transforms =
            type == ItemType.SmallenCake ? TeaRoomSceneManager.instance.SmallenCakeTrans :
            type == ItemType.LargenCake ? TeaRoomSceneManager.instance.LargenCakeTrans :            
            TeaRoomSceneManager.instance.MushroomTrans;

        // TODO プレイヤー数を取得
        int PlayerNum = 1;

        // 候補の数繰り返す
        foreach (Transform trans in Transforms)
        {
            var item = Instantiate(itemPrefab);
            item.transform.position = trans.position;

            if (itemList == null) itemList = new List<DrothyItem>();

            itemList.Add(item);

            NetworkServer.Spawn(item.gameObject);
        }
    }

    /// <summary>
    /// 配置済みアイテムを削除
    /// </summary>
    [Command]
    private void CmdRemoveItems()
    {
        for( int i = itemList.Count-1; i <= 0; --i )
        {
            var item = itemList[i];
            itemList.RemoveAt(i);
            NetworkServer.Destroy(item.gameObject);
        }
    }
}