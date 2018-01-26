using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

/// <summary>
/// イベントの進行を管理
/// 観測者からの命令で制御し、イベント関連のプレイヤーステートの監視なども行う
/// 基本的にサーバーで動かし、クライアント側にも命令を送る必要があるときだけRPCを使用する
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

	/// <summary>
	/// 現在のシーケンス
	/// 適宜クライアント側にも同期させる
	/// </summary>
	[SyncVar]
    private Sequence currentSequence = 0;
    public Sequence CurrentSequence { get { return currentSequence; } }

	/// <summary>
	/// アイテム種類
	/// 小さくなるケーキと巨大化ケーキ、きのこ
	/// </summary>
    private enum ItemType
    {
        SmallenCake,
        LargenCake,
        Mushroom,
    }

	/// <summary>
	/// 演出中か？
	/// 演出に入っているのにさらに同じ演出が始まらないように定義
	/// </summary>
    private bool inExpression = false;

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

    [ServerCallback]
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
            // オブザーバーについてはスキップ ただしオブザーバーのみの時は例外
            if (p.IsObserver && PlayerTest.list.Count >= 2) continue;

            bool inEffect = p.ItemEffectTimer > 0;

        //    Debug.Log( "ID" + p.netId.ToString() + " : " + ( inEffect ? "効果中" : "効果なし" ) );
            if (!inEffect) allPlayersInEffect = false;
        }

        if( allPlayersInEffect )
        {
            // 全員効果中なので次のシーケンスに移行
            ProceedSequence();
        }
    }

    /// <summary>
    /// オブザーバーのクライアントから呼ばれる用
    /// </summary>
    [Command]
    public void CmdProceedSequence()
    {
    //   Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

        ProceedSequence();
    }

    /// <summary>
    /// 自身で呼ぶ用
    /// </summary>
	[Server]
    private void ProceedSequence()
    {
	//	Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// シーケンスを+1する。最後のシーケンスだったら最初のシーケンスに戻る この値はSyncVarなのでクライアント側の値は勝手に同期される。ゆえにここでは操作しない
        currentSequence = (Sequence)(((int)currentSequence + 1) % (int)Sequence.Count_);

        Debug.Log("イベント " + currentSequence.ToString() + "の実行");

        switch (currentSequence)
        {
            case Sequence.CollapseGround_Event:

				// サーバーとクライアント両方でイベントを発生させる
				ExecCollapseFloorEvent();

                break;

            case Sequence.PopCakes1_Event:

                // 縮小化ケーキが出現する サーバー側のみでOK       
                CreateItems( ItemType.SmallenCake );

                break;

            case Sequence.SmallenDrothy_Event:

			// ドロシー縮小化イベント サーバーとクライアント両方でイベントを発生させる
                if (!inExpression)
                    StartCoroutine(ExpressionAndProceedSequence());

                break;

            case Sequence.PopCakes2_Event:

			// 巨大化ケーキ出現 サーバー側のみでOK
                CreateItems(ItemType.LargenCake);

                break;

            case Sequence.LargenDrothy_Event:

			// ドロシー巨大化イベント サーバーとクライアント両方でイベントを発生させる
                if (!inExpression)
                    StartCoroutine(ExpressionAndProceedSequence());

                break;

            case Sequence.PopMushrooms_Event:

			// きのこ出現 サーバー側のみでOK
                CreateItems(ItemType.Mushroom);

                break;

            case Sequence.Ending_Event:

			// エンディングから待機画面にもどる サーバーとクライアント両方でイベントを発生させる 

                if (!inExpression)
                    StartCoroutine(ExpressionAndProceedSequence());

                break;

            default:

                break;
        }

        // シーン切り替えを伴う場合は切り替え処理
        if (!currentSequence.ToString().Contains("Event"))
        {
			Debug.Log("シーン遷移 ->" + currentSequence.ToString());

			// シーンの切り替え
			var newSceneName = currentSequence.ToString();

			GotoNewScene( newSceneName );
        }
    }

	/// <summary>
	/// シーン遷移
	/// サーバーとクライアント両方で呼ぶために間にこれを挟む
	/// </summary>
	[Server]
	private void GotoNewScene( string newSceneName )
	{
	//	Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		StartCoroutine( ExecGotoNewScene( newSceneName ) );

		// クライアントでも同様のイベントを発生させる
		RpcGotoNewScene(newSceneName);
	}

	/// <summary>
	/// シーン遷移クライアント用
	/// RPCで直接コルーチンを呼べないため挟む
	/// </summary>
	[ClientRpc]
	private void RpcGotoNewScene( string newSceneName )
	{
	//	Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// ホストの場合は多重に呼ばれてしまうためなにもしない
		if( isServer && isClient )
		{
			return;			
		}

		StartCoroutine( ExecGotoNewScene( newSceneName ) );
	}

	/// <summary>
	/// シーン遷移
	/// サーバーでもクライアントでも呼ばれる
	/// </summary>
	private IEnumerator ExecGotoNewScene( string newSceneName )
	{
	//	Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		if (string.IsNullOrEmpty(newSceneName))
		{
			Debug.LogWarning(newSceneName + " シーンは存在しない ");
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

	/// <summary>
	/// 地面が崩れるイベント
	/// </summary>
	[Server]
	private void ExecCollapseFloorEvent()
	{
		// 地面が崩れるイベント
		var manager = GardenSceneManager.instance;
		if (manager == null) return;

		manager.PlayEvent();

		// クライアントでも同様のイベントを発生させる
		RpcExecCollapseFloorEvent();
	}

	/// <summary>
	/// 床が崩れるイベントRpc版
	/// 無印版と同じ処理なのであたまのいい方法があったら知りたい
	/// </summary>
	[ClientRpc]
	private void RpcExecCollapseFloorEvent()
	{
		// ホストの場合は多重に呼ばれてしまうためなにもしない
		if( isServer && isClient ) 
		{
			return;
		}

		// 地面が崩れるイベント
		var manager = GardenSceneManager.instance;
		if (manager == null) return;

		manager.PlayEvent();
	}

	/// <summary>
	/// 演出ののちシーケンスを進める
	/// </summary>
	[Server]
    private IEnumerator ExpressionAndProceedSequence()
    {
        inExpression = true;

        // TODO: 現在のシーケンスによって演出 サーバーとクライアント両方に対して行う
        yield return null;

        inExpression = false;

		ProceedSequence();
    }

    /// <summary>
    /// アイテム配置クライアントにも同期されるのでコールはサーバーのみ
    /// </summary>
	[Server]
    private void CreateItems( ItemType type )
    {
        if (TeaRoomSceneManager.instance == null) 
		{
			Debug.LogError("TeaRoomSceneManager.instanceがない");
			return;
		}

        // 出現候補を取得
		var transforms =
            type == ItemType.SmallenCake ? TeaRoomSceneManager.instance.SmallenCakeTrans :
            type == ItemType.LargenCake ? TeaRoomSceneManager.instance.LargenCakeTrans :            
            TeaRoomSceneManager.instance.MushroomTrans;

        Debug.Log(transforms.Length.ToString() + "この候補場所があるよ");

        // プレイヤー数を取得
        int playerNum = PlayerTest.list.Count - PlayerTest.PureObserverCount;
        Debug.Log(PlayerTest.list.Count.ToString() + " - " + PlayerTest.PureObserverCount.ToString() + " = " + playerNum.ToString());

		int setCount = 0;
        // 候補の数繰り返す
		foreach( Transform trans in transforms )
		{	
            var item = Instantiate(itemPrefab);
            item.transform.position = trans.position;

            if (itemList == null) itemList = new List<DrothyItem>();

            itemList.Add(item);

            NetworkServer.Spawn(item.gameObject);

            // 生成数がプレイヤーと一緒になったら終了
            if (++setCount >= playerNum)
            {
                Debug.Log(setCount.ToString() + "こ作った おしまい");
                break;
            }
            else
            {
                Debug.Log(setCount.ToString() + "こ作った まだつくる");
            }

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