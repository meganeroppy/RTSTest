using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionManager : MonoBehaviour {

	public static CameraPositionManager instance;

	[SerializeField]
	private Camera[] cameras;
	public Camera[] Cameras{ get{ return cameras; } }

	// Use this for initialization
	void Awake ()
	{
		instance = this;
	}

}
