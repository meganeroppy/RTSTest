using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEventTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void Go2()
	{
		Debug.Log( System.Reflection.MethodBase.GetCurrentMethod() );
	}

	public void Stand()
	{
//		Debug.Log( System.Reflection.MethodBase.GetCurrentMethod() );
	}
}
