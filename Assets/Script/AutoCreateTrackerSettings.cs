using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCreateTrackerSettings : MonoBehaviour {

	[SerializeField]
	bool active = false;

	[SerializeField]
	string[] objectNameList=null;

	[SerializeField]
	TrackerHostSettings trackerHost;

	string prefix = "Tracker_";

	// Use this for initialization
	void Start () {

		if (!active)
			return;

		foreach (string s in objectNameList) {
			if (s.Contains (",")) {
				var split = s.Split (',');
				int end = int.Parse (split [1]);
				for (int i = 1; i <= end; ++i) {
					var name = split [0] + i.ToString ();
					var obj = new GameObject (prefix + name).AddComponent<TrackerSettings> ();
					obj.HostSettings = trackerHost;
					obj.ObjectName = name;
					obj.transform.SetParent (transform, false);
				}
			} else {
				var obj = new GameObject (prefix + s).AddComponent<TrackerSettings> ();
				obj.HostSettings = trackerHost;
				obj.ObjectName = s;
				obj.transform.SetParent (transform, false);
			}
		}
	}	
}
