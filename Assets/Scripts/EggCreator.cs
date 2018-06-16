using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EggCreator : NetworkBehaviour
{
	enum Emote{
		Happy,
		Angry,
		Sad,
		Joy
	}

    [SerializeField]
	CopyTransform eggPrefab1=null;

	[SerializeField]
	CopyTransform eggPrefab2=null;
	[SerializeField]
	CopyTransform eggPrefab3=null;
	[SerializeField]
	CopyTransform eggPrefab4=null;
	[SerializeField]
	CopyTransform eggPrefab5=null;
	[SerializeField]
	CopyTransform eggPrefab6=null;
	[SerializeField]
	CopyTransform eggPrefab7=null;
	[SerializeField]
	CopyTransform eggPrefab8=null;
	[SerializeField]
	CopyTransform eggPrefab9=null;
	[SerializeField]
	CopyTransform eggPrefab10=null;
	[SerializeField]
	CopyTransform eggPrefab11=null;
	[SerializeField]
	CopyTransform eggPrefab12=null;
	[SerializeField]
	CopyTransform eggPrefab13=null;
	[SerializeField]
	CopyTransform eggPrefab14=null;
	[SerializeField]
	CopyTransform eggPrefab15=null;

	[SerializeField]
	CopyTransform toppogiPrefab1=null;

	[SerializeField]
	CopyTransform toppogiPrefab2=null;

	[SerializeField]
	CopyTransform horsePrefab=null;

	[SerializeField]
	CopyTransform earthPrefab=null;

	[SerializeField]
	CopyTransform chairPrefab=null;

	[SerializeField]
	CopyTransform handCameraPrefab=null;

	[SerializeField]
	CopyTransform hiraiBoyPrefab=null;

	[SerializeField]
	CopyTransform hiraiRHPrefab=null;

	[SerializeField]
	CopyTransform hiraiLHPrefab=null;

	[SerializeField]
	CopyTransform hiraiRFPrefab=null;

	[SerializeField]
	CopyTransform hiraiLFPrefab=null;

	[SerializeField]
	CopyTransform hiraiHeadPrefab=null;

	[SerializeField]
	Material[] materials=null;

	[SerializeField]
	Material[] nameMaterials=null;

	[SerializeField]
	Transform cameraPrefab;

	int currentCameraIndex = 0;

	List<Camera> cameraList = new List<Camera>();

	// Use this for initialization
    void Start()
    {
		CreateObjects ();
    }

	[SerializeField]
	float eggScale = 3f;

	[Server]
	void CreateObjects()
	{
		var trackerManage = GameObject.Find ("TrackerManage");
		if (!trackerManage) 
		{
			Debug.LogError ("TrackerManageが見つからない");
			return;
		}

		var trackerSettings = trackerManage.GetComponentsInChildren<TrackerSettings> ();

		foreach (TrackerSettings t in trackerSettings)
		{
			if (t.ObjectName.Contains ("Egg1") && t.ObjectName != ("Egg10") && t.ObjectName != ("Egg11") && t.ObjectName != ("Egg12") && t.ObjectName != ("Egg13") && t.ObjectName != ("Egg14")&& t.ObjectName != ("Egg15")) 
			{
				var obj = Instantiate (eggPrefab1).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg2") ) 
			{
				var obj = Instantiate (eggPrefab2).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg3") ) 
			{
				var obj = Instantiate (eggPrefab3).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg4") ) 
			{
				var obj = Instantiate (eggPrefab4).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg5") ) 
			{
				var obj = Instantiate (eggPrefab5).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg6") ) 
			{
				var obj = Instantiate (eggPrefab6).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg6") ) 
			{
				var obj = Instantiate (eggPrefab6).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg7") ) 
			{
				var obj = Instantiate (eggPrefab7).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg8") ) 
			{
				var obj = Instantiate (eggPrefab8).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg9") ) 
			{
				var obj = Instantiate (eggPrefab9).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg10") ) 
			{
				var obj = Instantiate (eggPrefab10).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg11") ) 
			{
				var obj = Instantiate (eggPrefab11).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg12") ) 
			{
				var obj = Instantiate (eggPrefab12).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg13") ) 
			{
				var obj = Instantiate (eggPrefab13).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg14") ) 
			{
				var obj = Instantiate (eggPrefab14).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Egg15") ) 
			{
				var obj = Instantiate (eggPrefab15).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= eggScale;
				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Toppogi1") ) 
			{
				var obj = Instantiate (toppogiPrefab1).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= 1.5f;

				var mesh = obj.GetComponentInChildren<MeshRenderer> ();
				if (mesh == null)
					return;

				var keyStr = t.ObjectName.Substring (t.ObjectName.Length - 1);

				int result;
				if (!int.TryParse (keyStr, out result)) {
					return;
				}

				NetworkServer.Spawn (obj.gameObject);
			}
			else if (t.ObjectName.Contains ("Toppogi2") ) 
			{
				var obj = Instantiate (toppogiPrefab2).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= 1.5f;

				var mesh = obj.GetComponentInChildren<MeshRenderer> ();
				if (mesh == null)
					return;

				var keyStr = t.ObjectName.Substring (t.ObjectName.Length - 1);

				int result;
				if (!int.TryParse (keyStr, out result)) {
					return;
				}

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Horse") ) 
			{
				var obj = Instantiate (horsePrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Earth") ) 
			{
				var obj = Instantiate (earthPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Chair") ) 
			{
				var obj = Instantiate (chairPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiBody") ) 
			{
				var obj = Instantiate (hiraiBoyPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiRH") ) 
			{
				var obj = Instantiate (hiraiRHPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiLH") ) 
			{
				var obj = Instantiate (hiraiLHPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiRF") ) 
			{
				var obj = Instantiate (hiraiRFPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiLF") ) 
			{
				var obj = Instantiate (hiraiLFPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("HiraiHead") ) 
			{
				var obj = Instantiate (hiraiHeadPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Camera") ) 
			{
				var obj = Instantiate (handCameraPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;

				var cam = obj.transform.GetComponentInChildren<Camera> ();
				if (cam != null) {
					cameraList.Add (cam);
				}

				NetworkServer.Spawn (obj.gameObject);
			}
		}

		// 固定カメラを生成
		StartCoroutine( CreateCamera() );
	}

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.LeftArrow))
			SwitchCamera (false);
		if (Input.GetKeyDown (KeyCode.RightArrow))
			SwitchCamera (true);
		if (Input.GetKeyDown (KeyCode.C)) {
			camName.enabled = !camName.enabled;
		}	

		if( Input.GetKeyDown(KeyCode.Q ) )
		{
			SetEffect (Emote.Happy);
		}
		if( Input.GetKeyDown(KeyCode.W ) )
		{
			SetEffect (Emote.Angry);
		}
		if( Input.GetKeyDown(KeyCode.E ) )
		{
			SetEffect (Emote.Sad);
		}
		if( Input.GetKeyDown(KeyCode.R ) )
		{
			SetEffect (Emote.Joy);
		}

		if( Input.GetKeyDown(KeyCode.Alpha0 ) )
		{
			currentCameraIndex = 0;
			SetActiveCamera ();
		}
		if( Input.GetKeyDown(KeyCode.Alpha1 ) )
		{
			currentCameraIndex = 1;
			SetActiveCamera ();
		}
		if( Input.GetKeyDown(KeyCode.Alpha2 ) )
		{
			currentCameraIndex = 2;
				SetActiveCamera ();
		}
		if( Input.GetKeyDown(KeyCode.Alpha3 ) )
		{
			currentCameraIndex = 3;
				SetActiveCamera ();
		}
		if( Input.GetKeyDown(KeyCode.Alpha4 ) )
		{
			currentCameraIndex = 4;
				SetActiveCamera ();
		}
		if( Input.GetKeyDown(KeyCode.Alpha5 ) )
		{
			currentCameraIndex = 5;
			SetActiveCamera ();
		}

	}

		
	IEnumerator CreateCamera()
	{
		while (CameraPositionManager.instance == null) {
			yield return null;
		}
	
		var c = CameraPositionManager.instance; 

		foreach( Camera t in c.Cameras )
		{
			cameraList.Add (t);
		}

		SwitchCamera (true);
	}

	/// <summary>
	/// カメラ切り替え
	/// </summary>
	void SwitchCamera( bool forward )
	{
		if (cameraList == null)
			return;

		if( forward )
			currentCameraIndex = (currentCameraIndex + 1) % cameraList.Count;
		else
			currentCameraIndex = (currentCameraIndex + cameraList.Count - 1) % cameraList.Count;

		SetActiveCamera ();
	}

	void SetActiveCamera()
	{
		for ( int i=0 ; i < cameraList.Count ; ++i) {

			bool flag = i == currentCameraIndex;

			cameraList [i].enabled = flag;

			if (cameraList [i].transform.childCount <= 0) {
				Debug.LogWarning ( cameraList[i].name + "は子要素なし");
				continue;
			}
			var efsCam = cameraList [i].transform.GetChild (0).GetComponent<Camera> ();

			if (efsCam == null) {
				Debug.LogWarning ( efsCam.name + "にカメラコンポーネントなし");
				continue;
			}
			efsCam.enabled = flag;
		}

		UpdateUi ();

	}

	[SerializeField]
	UnityEngine.UI.Text camName;

	void UpdateUi()
	{
		var currentCam = cameraList [currentCameraIndex];
		camName.text = currentCam.name;
	}

	[SerializeField]
	LayerMask effectTargetmask;
	[SerializeField]
	string effectTargetMaskName = "Egg";

	void SetEffect( Emote emote )
	{
		int maskNo = LayerMask.NameToLayer (effectTargetMaskName);
		int mask = 1 << maskNo;
		var currentCam = cameraList [currentCameraIndex];
		RaycastHit hit;
		if ( !Physics.Raycast (currentCam.transform.position, currentCam.transform.forward, out hit, 10f, mask)) {
			Debug.Log ("ヒットなし");
			return;
		}

		Debug.Log (hit.transform.gameObject.name + "にヒット");

		GameObject obj=null;
		Vector3 position = hit.collider.transform.position;

		if( emote == Emote.Happy )
		{
			obj = Instantiate (happy);
		}

		else if( emote == Emote.Angry )
		{
			obj = Instantiate (angry);
		}

		else if( emote == Emote.Sad )
		{
			obj = Instantiate (sad);
		}

		else if( emote == Emote.Joy )
		{
			obj = Instantiate (joy);
		}

		obj.transform.position = position;
		obj.transform.forward = cameraList [currentCameraIndex].transform.forward;

	}

	[SerializeField]
	GameObject happy;

	[SerializeField]
	GameObject angry;

	[SerializeField]
	GameObject sad;

	[SerializeField]
	GameObject joy;

}
