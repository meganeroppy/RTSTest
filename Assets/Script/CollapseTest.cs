using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseTest : MonoBehaviour {

	public FracturedObject[] objects;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Collapse()
	{
		foreach( FracturedObject o in objects )
		{
//			o.
			o.CollapseChunks();
		}
	}
}
