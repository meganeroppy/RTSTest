using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Config
//{
	public class LoadedConfig  {
		public static LoadedConfig instance;

		public LoadedConfig()
		{
			data = new List<KeyValuePair<string, string>>();
		}
		public string serverId;

		public List<KeyValuePair<string, string>> data;
	}

	public struct PresetConfig
	{
	//	public readonly string headPrefix = "Head";
	//	public readonly string rightHandPrefix = "RH";
	//	public readonly string leftHandPrefix = "LH";
	}
//}
