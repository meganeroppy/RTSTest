using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {

	[SerializeField]
	string nextSceneName = "";

	[SerializeField]
	Text fpsLabel;

	private readonly List<float> deltaTimeList = new List<float>();
	private float fps;
	private int samplingSize = 30;

	#region osc

	private long lastTimeStamp;

	#endregion osc

	private void Awake()
	{
		// コンフィグファイルを読み込み
		ReadFile();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		CheckStartFlag();
		UpdateFps();
		UpdateUi();
	}

	void CheckStartFlag()
	{
		if( Input.GetKeyDown(KeyCode.Space) )
		{
			SceneManager.LoadScene( nextSceneName );
		}
	}

	void UpdateUi()
	{
		fpsLabel.text = "FPS : " + fps.ToString(); 
	}

	void UpdateFps()
	{
		deltaTimeList.Add( Time.deltaTime );
		if( deltaTimeList.Count > samplingSize )
		{
			deltaTimeList.RemoveAt(0);
		}

		float sum = 0;
		int count = 0;
		foreach( float f in deltaTimeList )
		{
			sum += f;
			count++;
		}

		float average = sum / (float)count;
		fps = 1 /average;
	}

	/// <summary>
	/// 受信していないかチェック
	/// </summary>
	private void CheckOscReceive()
	{
		// チェックするアドレスを指定する
		string checkAddress = "start";

		//  受信データの更新
		OSCHandler.Instance.UpdateLogs();

		//  受信データの解析
		foreach (KeyValuePair<string, ServerLog> item in OSCHandler.Instance.Servers)
		{
			for (int i = 0; i < item.Value.packets.Count; i++)
			{
				var packet = item.Value.packets[i];
				if (lastTimeStamp < packet.TimeStamp)
				{
					lastTimeStamp = packet.TimeStamp;
					//  アドレスパターン（文字列）
					string address = packet.Address;

					// 適切なアドレスが来ていたら0番目の値でフラグを更新
					if (address.Contains(checkAddress))
					{
						byte result;
						if (byte.TryParse(packet.Data[0].ToString(), out result))
						{
							if (result == 1)
							{
								// ダミーのパケットを挿入する
								var dammyPacket = packet;
								dammyPacket.Data[0] = 0;
								item.Value.packets.Add(dammyPacket);

								// ゲーむ開始
//								StartCoroutine(ExpressionAndLoadScene());
								break;
							}
						}
					}
				}
			}
		}
	}

	// 読み込み関数
	void ReadFile()
	{
		string str = "";

		// Config.txtファイルを読み込む
		string fileName = "Config.txt";

		FileInfo fi = new FileInfo(Application.dataPath + "/" + fileName);
		if( !fi.Exists )
		{
			Debug.LogWarning(fileName + "が存在しません。通常タイトル画面を表示します");
//			SetUpTitle();
			return;
		}

		//     try
		//       {
		// 一行毎読み込み
		using (StreamReader sr = new StreamReader(fi.OpenRead(), Encoding.UTF8))
		{
			str = sr.ReadToEnd();
		}
		//        }
		//        catch (Exception e)
		//        {
		// 改行コード
		//            str += SetDefaultText();
		//        }

		var strs = str.Split('\n');

		foreach( string s in strs )
		{
			SaveToConfig( s );
		}

		Debug.Log(str);
	}

	private void SaveToConfig( string str )
	{
		var strs = str.Split('=');
		if( strs.Length != 2 )
		{
			Debug.LogWarning("コンフィグとして不正なフォーマット -> [" + str + "]");
			return;
		}

		var pair = new KeyValuePair<string, string>( strs[0], strs[1] );

		if( LoadedConfig.instance == null )
		{
			LoadedConfig.instance = new LoadedConfig();
		}
		LoadedConfig.instance.data.Add( pair );
	}

	// 改行コード処理
	string SetDefaultText()
	{
		return "C#あ\n";
	}
}
