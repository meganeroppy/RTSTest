using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSceneManager : MonoBehaviour 
{
	private string callMethodName = "StartFalling";
	// Use this for initialization
	void Start () {
		
		var drothyTeam = GameObject.FindGameObjectsWithTag("Drothy");
		foreach( GameObject g in drothyTeam )
		{
			g.SendMessage(callMethodName);
			Debug.Log(g.name + "に対して" + callMethodName + "の呼び出しを要求");
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
