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
    public enum Role
    {
        Server,
        Host,
        Client,
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
            default:
                StartClient();
                break;            
        }
    }
}
