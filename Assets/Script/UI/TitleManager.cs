using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour {

	[SerializeField]
	string nextSceneName = "";

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		CheckStartFlag();
	}

	void CheckStartFlag()
	{
		if( Input.GetKeyDown(KeyCode.Space) )
		{
			SceneManager.LoadScene( nextSceneName );
		}
	}
}
