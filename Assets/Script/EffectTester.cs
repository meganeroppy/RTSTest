using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectTester : MonoBehaviour {

	[SerializeField]
	GameObject happy;

	[SerializeField]
	GameObject angry;

	[SerializeField]
	GameObject sad;

	[SerializeField]
	GameObject joy;

	// Update is called once per frame
	void Update () {
		if( Input.GetKeyDown(KeyCode.Q ) )
		{
			Instantiate( happy, transform.position, transform.rotation);
		}

		if( Input.GetKeyDown(KeyCode.W ) )
		{
			Instantiate( angry, transform.position, transform.rotation);
		}

		if( Input.GetKeyDown(KeyCode.E ) )
		{
			Instantiate( sad, transform.position, transform.rotation);
		}
			
		if( Input.GetKeyDown(KeyCode.R ) )
		{
			Instantiate( joy, transform.position, transform.rotation);
		}

	}
}
