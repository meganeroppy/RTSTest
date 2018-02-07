using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ネットワーク関連処理
/// NetworkManagaer継承するのが正義かも
/// </summary>
public class RtsTestNetworkManager : NetworkBehaviour 
{
    /// <summary>
    /// 自身のインスタンス
    /// </summary>
    public static RtsTestNetworkManager instance;

	private NetworkManager nManager;

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
	/// 観測者フラグ
	/// 後々動的に設定できるようにする
	/// </summary>
	[SerializeField]
	private bool isObserver = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool IsObserver
	{
		get { return isObserver; }
	}

	/// <summary>
	/// 観測者タイプ
	/// </summary>
	public enum ObserverType
	{
		Default,	// 通常の観測者 キーボードで操作する
		Participatory, // 参加型観測者 他のプレイヤーと同様にプレイスペース内に入りトラッキングにより操作する InputTypeの強制キーボード操作が有効でもこちらが優先される
	}

	[SerializeField]
	private ObserverType observerType = ObserverType.Default;

	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public ObserverType MyObserverType{get{ return observerType;}}


	/// <summary>
	/// 入力モード
	/// 実稼働時にはデフォルトの使用を前提とする
	/// </summary>
	public enum InputMode
	{
		ForceByKeyboard, // 強制キーボード操作 プレイヤーであってもキーボードで操作する
		ForceByTracking, // 強制トラッカー依存操作 観測者であってもトラッカー経由での操作となる ObserverTypeの参加型観測者が有効の時は無意味になる
		Default, // 基本操作 プレイヤーはトラッカー依存操作 & 観測者はキーボード操作 ただし参加型観測者の場合は観測者もトラッカー依存操作
	}

	[SerializeField]
	private InputMode inputMode = InputMode.Default;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public InputMode MyInputMode{ get{ return inputMode;} }

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
	/// 自身であってもドロシーを表示するか？
	/// 完全に開発用
	/// 「ドロシー」でなく自身を表示にし、いもむしの表示にも対応させるとよいかも
	/// </summary>
	[SerializeField]
	private bool forceDisplayDrothy = false;
	/// <summary>
	/// ！ローカルでのみ使用すること！
	/// リモートで使用するとえらいことになる
	/// </summary>
	public bool ForceDisplayDrothy
	{
		get { return forceDisplayDrothy; }
	}

	/// <summary>
	/// 自身の役割を実行済みか？
	/// </summary>
	private bool executedOwnRole = false;

    private void Awake()
    {
        instance = this;
    }

    // Use this for initialization
    void Start()
    {
        nManager = GetComponent<NetworkManager>();

        // 何か文字列が入っていたらサーバアドレスを上書き
        if (!string.IsNullOrEmpty(localServerAddress))
        {
            nManager.networkAddress = localServerAddress;
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
                nManager.StartServer();
                break;
            case Role.Host:
                nManager.StartHost();
                break;
            case Role.Client:
            default:
                nManager.StartClient();
                break;            
        }
    }
}
