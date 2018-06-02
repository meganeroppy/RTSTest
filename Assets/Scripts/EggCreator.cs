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

				NetworkServer.Spawn (obj.gameObject);
			}
		}
	}
}
