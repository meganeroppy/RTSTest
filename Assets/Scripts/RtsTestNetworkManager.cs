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
    public int GetPlayerId()
    {
        return playerId;
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
    /// 後々動的に設定できるようにする
    /// </summary>
    [SerializeField]
    private string localServerAddress;

    [SerializeField]
    private bool autoExecRole = false;


    /// <summary>
    /// 観測者フラグ
    /// 後々動的に設定できるようにする
    /// </summary>
    [SerializeField]
    private bool isObserver;
    public bool IsObserver
    {
        get { return isObserver; }
    }

    /// <summary>
    /// 強制トラッカー依存
    /// 有効にすると観測者フラグが立っていてもトラッカー依存の操作系になる
    /// 後々動的に設定できるようにする
    /// </summary>
    [SerializeField]
    private bool forceRelatedToTracking;
    public bool ForceRelatedToTracking
    {
        get { return forceRelatedToTracking; }
    }

    /// <summary>
    /// RTS空間内のプレイヤーの動きをシミュレートするか？
    /// </summary>
    [SerializeField]
    private bool simulateRtsMovement = false;
    public bool SimulateRtsMovement
    {
        get { return simulateRtsMovement; }
    }

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
