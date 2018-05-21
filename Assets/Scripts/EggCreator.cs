using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EggCreator : NetworkBehaviour
{
    [SerializeField]
	CopyTransform eggPrefab;

    // Use this for initialization
    void Start()
    {
		CreateEggs ();
    }

	[Server]
	void CreateEggs()
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

				NetworkServer.Spawn (obj.gameObject);
			}
			else if ( t.ObjectName.Contains ("Wand") ) 
			{
				var obj = Instantiate (eggPrefab).GetComponent<CopyTransform>();
				obj.copySource = t.gameObject;
				obj.transform.localScale *= 1.2f;

				NetworkServer.Spawn (obj.gameObject);
			}
		}
	}
}
