using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionManager : MonoBehaviour {

	public static CameraPositionManager instance;

	public Camera[] Cameras{ get; private set; }

	// Use this for initialization
	void Awake ()
	{
		instance = this;
	}

	void Start()
	{
		Cameras = transform.GetComponentsInChildren<Camera> ();
	}
}
