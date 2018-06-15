using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EggCreator : NetworkBehaviour
{
    [SerializeField]
	CopyTransform eggPrefab;

	[SerializeField]
	CopyTransform horsePrefab;

	[SerializeField]
	CopyTransform earthPrefab;

	[SerializeField]
	CopyTransform chairPrefab;

	[SerializeField]
	CopyTransform handCameraPrefab;

	[SerializeField]
	Material[] materials;

	[SerializeField]
	Transform cameraPrefab;

	int currentCameraIndex = 0;

	List<Camera> cameraList = new List<Camera>();

	// Use this for initialization
    void Start()
    {
		CreateObjects ();
    }

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
			if (t.ObjectName.Contains ("Egg") ) 
			{
				var obj = Instantiate (eggPrefab).GetComponent<CopyTransform>();
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

				var newMat = materials[result % materials.Length];

				mesh.material = newMat;

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Wand") ) 
			{
				var obj = Instantiate (eggPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= 1.8f;

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
		
		for ( int i=0 ; i < cameraList.Count ; ++i) {
			cameraList [i].enabled = i == currentCameraIndex;
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
}
