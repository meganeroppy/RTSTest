using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ネットワーク関連処理
/// NetworkManagaer継承するのが正義かも
/// </summary>
public class RtsTestNetworkManager : NetworkManager 
{
    /// <summary>
    /// 自身のインスタンス
    /// </summary>
    public static RtsTestNetworkManager instance;

    [SerializeField]
    private int playerId = 1;
    public int PlayerId
	{
		get{
        	return playerId;
    	}
	}

	/// <summary>
	/// オートの挙動：
	/// まずクライアントとして定義済みのアドレスに接続しようとする
	/// すべての定義済みアドレスに接続しようとして失敗したら、自身がホストになる
	/// </summary>
    public enum Role
    {
        Server,
        Host,
		Client,
		Auto,
    }
	public Role GetRole()
	{
		return role;
	}

	[SerializeField]
	private Role role;

	/// <summary>
	/// 開始時に自動で自身の役割を実行するか？
	/// </summary>
	[SerializeField]
	private bool autoExecRole = false;

	/// <summary>
	/// 定義済みサーバーアドレス
	/// オート設定の時だけ使用する
	/// </summary>
	[SerializeField]
	private string[] definedServerAddress;	

	/// <summary>
	/// 後々アプリケーションと同じディレクトリに保存したファイルなどから設定できるようにする
	/// </summary>
	[SerializeField]
	private string localServerAddress;

	/// <summary>
	/// ナビゲーターフラグ
	/// 後々動的に設定できるようにする
	/// </summary>
	[SerializeField]
	private bool isNavigator = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool IsNavigator
	{
		get { return isNavigator; }
	}

	/// <summary>
	/// ナビゲータータイプ
	/// 基本は参加型
	/// </summary>
	public enum NavigatorType
	{
		Participatory, // 参加型 他のプレイヤーと同様にプレイスペース内に入りトラッキングにより操作する InputTypeの強制キーボード操作が有効でもこちらが優先される
		Remote,	// キーボードで遠隔操作
	}

	[SerializeField]
	private NavigatorType navigatorType = NavigatorType.Remote;

	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public NavigatorType MyNavigatorType{get{ return navigatorType;}}

	/// <summary>
	/// 入力モード
	/// 実稼働時にはデフォルト1の使用を前提とする
	/// </summary>
	public enum InputMode
	{
		Defalut, // プレイヤー・ナビゲーター共にトラッカー依存で操作する
		RemoteNavigator, // プレイヤーはトラッカー依存操作 & ナビゲーターはキーボード操作
		ForceByKeyboard, // 強制キーボード操作 プレイヤーであってもキーボードで操作する デバッグ用
	}

	[SerializeField]
	private InputMode inputMode = InputMode.RemoteNavigator;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public InputMode MyInputMode{ get{ return inputMode;} }

	/// <summary>
	/// プレイヤーのIDを表示するか？
	/// </summary>
	[SerializeField]
	private bool showPlayerId = false;
	public bool ShowPlayerId{ get{ return showPlayerId;}}

    /// <summary>
    /// RTS空間内のプレイヤーの動きをシミュレートするか？
	/// RTS設備がない環境での開発用
    /// </summary>
    [SerializeField]
    private bool simulateRtsMovement = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool SimulateRtsMovement
    {
        get { return simulateRtsMovement; }
    }

	/// <summary>
	/// 脚の動きのシミュレーションを有効にするか？
	/// </summary>
	[SerializeField]
	private bool useSimulateFoot = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool UseSimulateFoot
	{
		get { return useSimulateFoot; }
	}

	/// <summary>
	/// 自身であっても自身のビジュアルを表示するか？
	/// 開発用
	/// </summary>
	[SerializeField]
	private bool forceDisplayAvatar = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool ForceDisplayAvatar
	{
		get { return forceDisplayAvatar; }
	}

	/// <summary>
	/// 自身の役割を実行済みか？
	/// </summary>
	private bool executedOwnRole = false;

    /// <summary>
    /// 初期シーンリスト
    /// </summary>
    public enum FirstSceneEnum
    {
        Garden,
        Template,
    }
    /// <summary>
    /// 最初に遷移するシーン
    /// </summary>
    [SerializeField]
    private FirstSceneEnum firstScene = FirstSceneEnum.Template;
    public FirstSceneEnum FirsScene { get { return firstScene; } }

    /// <summary>
    /// アバター種類
    /// ドロシー or Unityちゃん
    /// </summary>
    public enum AvatarTypeEnum
    {
        Drothy,
        UnityChan
    }
    [SerializeField]
    private AvatarTypeEnum avatarType = AvatarTypeEnum.UnityChan;
    public AvatarTypeEnum AvatarType { get { return avatarType; } }

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        // 何か文字列が入っていたらサーバアドレスを上書き
        if (!string.IsNullOrEmpty(localServerAddress))
        {
            networkAddress = localServerAddress;
        }

        if (autoExecRole) ExecRole();
	}
	
	// Update is called once per frame
	void Update ()
    {
        CheckInput();
	}

    private void CheckInput()
    {
        if( Input.GetKeyDown( KeyCode.T ) )
        {
            ExecRole();
        }
    }

    public void ExecRole()
    {
        if (executedOwnRole) return;

        executedOwnRole = true;

        switch( role )
        {
            case Role.Server:
                StartServer();
                break;
            case Role.Host:
                StartHost();
                break;
            case Role.Client:
				StartClient();
				break;
			case Role.Auto:			
            default:
				StartCoroutine(ExecAutoRoleProcedure());
                break;            
        }
    }

	public override void OnClientDisconnect (NetworkConnection conn)
	{
		Debug.Log( System.Reflection.MethodBase.GetCurrentMethod()); 

		base.OnClientDisconnect (conn);

		disconnected = true;
	}
	private bool disconnected = false;
	private bool connected = false;

	public override void OnClientConnect (NetworkConnection conn)
	{
		base.OnClientConnect (conn);
		connected = true;
	}

	/// <summary>
	/// オート時の処理を実行する
	/// ①：定義済みアドレスにクライアントとしての接続を試みる
	/// ②：接続に失敗したら次の定義済みアドレスを接続先として指定する
	/// ③：①、②を定義済みアドレスを全て試すまで繰り返す
	/// ④：③をプレイヤーIDの回数繰り返す
	/// ⑤：④まで終わってまだ接続できていなかったら自身がホストになる
	/// </summary>
	/// <returns>The auto role procedure.</returns>
	private IEnumerator ExecAutoRoleProcedure()
	{
		int MaxLoop = Mathf.Abs(playerId);

		Debug.Log("オート処理開始 プレイヤーIDは" + playerId.ToString() + "なので" + MaxLoop.ToString() + "回試行する");

		int retryCount = 0;
		int retryLoopCount = 0;

		while( retryLoopCount < MaxLoop && !connected)
		{
			localServerAddress = definedServerAddress[ retryCount ];
			StartClient();

			Debug.Log( localServerAddress + "にクライアントとして接続中" );
			while( !connected && !disconnected )
			{
				yield return null;
			}

			if( connected )
			{
				Debug.Log( localServerAddress + "にクライアントとして接続完了 オート処理を終了" );
				break;
			}

			if( disconnected )
			{
				Debug.Log( localServerAddress + "へクライアントとして接続失敗 次の処理を実行" );
				retryCount++;
				if( retryCount >= definedServerAddress.Length )
				{
					retryLoopCount++;
					retryCount = 0;
				}
			}
		}

		Debug.Log( "クライアントとしての接続処理失敗 自身がホストになる" );
		StartHost();
	}
}
