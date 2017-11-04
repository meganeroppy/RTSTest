using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedConfig  {
	public static LoadedConfig instance;

	public LoadedConfig()
	{
		data = new List<KeyValuePair<string, string>>();
	}
	public string serverId;
	public List<KeyValuePair<string, string>> data;
}
