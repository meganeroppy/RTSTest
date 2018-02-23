﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Effect : NetworkBehaviour 
{
	[SerializeField]
	private float lifeTime = 2f;

	[SerializeField]
	private bool spawnOnNetwork = true;

	private float timer = 0;

	// Use this for initialization
	void Start () {
		timer = 0;	
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if( timer >= lifeTime )
		{
			if( spawnOnNetwork )
			{
				NetworkServer.Destroy( gameObject );
			}
			else
			{
				Destroy( gameObject );
			}
		}
	}
}
