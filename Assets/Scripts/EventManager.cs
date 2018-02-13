using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

/// <summary>
/// イベントの進行を管理
/// ナビゲーターからの命令で制御し、イベント関連のプレイヤーステートの監視なども行う
/// 基本的にサーバーで動かし、クライアント側にも命令を送る必要があるときだけRPCを使用する
/// </summary>
public class EventManager : NetworkBehaviour
{
    /// <summary>
    /// 自身へのインスタンス
    /// </summary>
    public static EventManager instance;

    [SerializeField]
    private DrothyItem cakePrefab;

    [SerializeField]
    private DrothyItem mushroomPrefab;

    /// <summary>
    /// 出現しているアイテムのリスト
    /// </summary>
	private List<DrothyItem> itemList;
	public List<DrothyItem> ItemList { get { return itemList; } }

    /// <summary>
    /// シーン切り替え時の
    /// 通常->暗転
    /// 暗転->通常
    /// にかかる時間
    /// </summary>
    private float sceneChangeFadeDuration = 1f;

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

    private AudioSource audioSource;

    /// <summary>
    /// ドロシーが縮小化するときの効果音
    /// </summary>
    [SerializeField]
    private AudioClip itemEffectToSmall;

    /// <summary>
    /// ドロシーが巨大化するときの効果音
    /// </summary>
    [SerializeField]
    private AudioClip itemEffectToLarge;

	/// <summary>
	/// 公園シーンでの地面崩壊イベント開始から次シーンに遷移するまでの秒数
	/// </summary>
	private float collapseFloorEventWait = 17f;

	/// <summary>
	/// 落下シーンで次シーンに遷移するまでの秒数
	/// </summary>
	private float fallingEventWait = 20f;

	/// <summary>
	/// ティールーム系シーンでアイテムが出現するまでの秒数
	/// </summary>
	private float itemPopEventWait = 10f;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
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
            // 参加型でないオブザーバーはスキップ ただしオブザーバーのみの時は例外
            if (p.IsNavigator && p.NavigatorType != RtsTestNetworkManager.NavigatorType.Participatory && PlayerTest.list.Count >= 2) continue;

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
	/// 強制的にシーケンスを進める
	/// ナビゲータが強制的に進行させたい時に使う
	/// </summary>
	[Server]
	public void ForceProceedSequence()
	{
		// コルーチンが動いていたら停止

		// ここはなくても一緒かも？
		{
			if( waitAndExecCoroutine != null )
			{
				StopCoroutine( waitAndExecCoroutine );
				waitAndExecCoroutine = null;
			}
		}
		StopAllCoroutines();

		// イベント効果音の再生を停止
		RpcStopSound();

		// 演出を中断するので演出中フラグを下げる
		inExpression = false;

		ProceedSequence();
	}

    /// <summary>
    /// シーケンスを進める
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

			// 特定のシーケンスに遷移する時は時間経過によりさらに次のシーケンスに進める
			switch( currentSequence )
			{
			case Sequence.Falling:
				WaitAndProceedSequence( fallingEventWait );
				break;
			case Sequence.TeaRoom:
			case Sequence.TeaRoomLarge:
			case Sequence.TeaRoomSmall:
				WaitAndProceedSequence( itemPopEventWait );
				break;
			default:
				break;
			}
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

        // 暗転 実際に暗転処理をコールするのはサーバーのみ
        float progress = 0;
        while (progress < 1f)
        {
            if (isServer)
            {
                var newColor = new Color(0f, 0f, 0f, progress);
                PlayerTest.list.ForEach(p => p.RpcSetCameraMaskColor(newColor));
            }
            progress += Time.deltaTime / sceneChangeFadeDuration;
            yield return null;            
        }

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

        // 暗転解除 実際に暗転処理をコールするのはサーバーのみ
        progress = 0;
        while (progress < 1f)
        {
            if (isServer)
            {
                var newColor = new Color(0f, 0f, 0f, 1f - progress);
                PlayerTest.list.ForEach(p => p.RpcSetCameraMaskColor(newColor));
            }
            progress += Time.deltaTime / sceneChangeFadeDuration;
            yield return null;
        }
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

		WaitAndProceedSequence(collapseFloorEventWait);

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

		WaitAndProceedSequence(collapseFloorEventWait);
	}

	/// <summary>
	/// 待機してから進めるコルーチン
	/// </summary>
	IEnumerator waitAndExecCoroutine = null;

	private void WaitAndProceedSequence( float wait )
	{
		waitAndExecCoroutine = WaitForNextSequence(wait);
		StartCoroutine( waitAndExecCoroutine );
	}

	/// <summary>
	/// 次のシーケンスまで待機してコルーチンを削除
	/// </summary>
	private IEnumerator WaitForNextSequence(float wait)
	{
		yield return new WaitForSeconds(wait);

		ProceedSequence();

		waitAndExecCoroutine = null;
	}

    /// <summary>
    /// 演出ののちシーケンスを進める
    /// </summary>
    [Server]
    private IEnumerator ExpressionAndProceedSequence()
    {
        inExpression = true;

        // 演出
        {
            // TODO: 現在のシーケンスによって演出 サーバーとクライアント両方に対して行う
            if (currentSequence == Sequence.SmallenDrothy_Event)
            {
                // ドロシーが縮小

                // 効果音
                RpcPlaySmallenSound();

                // ビジュアルエフェクト

                // 暗転

                yield return new WaitForSeconds(5);

                RpcStopSound();
            }
            else if (currentSequence == Sequence.LargenDrothy_Event)
            {
                // ドロシーが巨大化

                // 効果音
                RpcPlayLargenSound();

                // ビジュアルエフェクト

                yield return new WaitForSeconds(5);

                RpcStopSound();
            }
            else if (currentSequence == Sequence.Ending_Event)
            {

				// 効果音
				// TODO: もっとエンディングっぽい音にする
				RpcPlaySmallenSound();

                // ビジュアルエフェクト

                yield return new WaitForSeconds(5);

				RpcStopSound();
            }

        }
        inExpression = false;

		ProceedSequence();
    }

    /// <summary>
    /// アイテム配置クライアントにも同期されるのでコールはサーバーのみ
    /// </summary>
	[Server]
    private void CreateItems( ItemType type )
    {
		Debug.Log(System.Reflection.MethodBase.GetCurrentMethod());

		// すでに配置されたアイテムがあればリストを削除
		RemoveItems();

		// リストを新しく作る
		itemList = new List<DrothyItem>();

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

        var prefab = type == ItemType.Mushroom ? mushroomPrefab : cakePrefab;

        // プレイヤー数を取得
        int playerNum = PlayerTest.list.Count - PlayerTest.PureNavigatorCount;
        Debug.Log(PlayerTest.list.Count.ToString() + " - " + PlayerTest.PureNavigatorCount.ToString() + " = " + playerNum.ToString());

		int setCount = 0;
        // 候補の数繰り返す
		foreach( Transform trans in transforms )
		{	
            var item = Instantiate(prefab);
            item.transform.position = trans.position;

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

		Debug.Log( "アイテムの数 -> " + itemList.Count.ToString() );
    }

    /// <summary>
    /// 配置済みアイテムを削除
    /// </summary>
	[Server]
    private void RemoveItems()
    {
		if( itemList == null ) return;

        for( int i = itemList.Count-1; i >= 0; --i )
        {
            var item = itemList[i];
            itemList.RemoveAt(i);
			if( item != null )
			{
            	NetworkServer.Destroy(item.gameObject);
			}
        }

		itemList = null;
    }

    /// <summary>
    /// ドロシー縮小化サウンド再生
    /// </summary>
    [ClientRpc]
    private void RpcPlaySmallenSound()
    {
        audioSource.clip = itemEffectToSmall;
        audioSource.Play();
    }

    /// <summary>
    /// ドロシー巨大化サウンド再生
    /// </summary>
    [ClientRpc]
    private void RpcPlayLargenSound()
    {
        audioSource.clip = itemEffectToLarge;
        audioSource.Play();
    }

    /// <summary>
    /// サウンド停止
    /// </summary>
    [ClientRpc]
    private void RpcStopSound()
    {
        audioSource.Stop();
    }
}