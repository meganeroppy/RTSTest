using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateWalk : MonoBehaviour
{
	public Transform[] feet;

	public float groundHeight = 0;
	public float height = 1f;
	public float speed = 0.1f;

	private int idx = 0;
	// Use this for initialization
	void Start ()
	{
		StartCoroutine( Exec() );
	}
	
	IEnumerator Exec()
	{
		var foot = feet[ idx ];

		// ascend
		while( foot.position.y < groundHeight + height )
		{
			foot.position += Vector3.up *  speed * Time.deltaTime;
			yield return null;
		}

		// descend
		while( foot.position.y > groundHeight  )
		{
			foot.position += -Vector3.up *  speed * Time.deltaTime;
			yield return null;
		}

		idx = (idx+1) % feet.Length;

		StartCoroutine( Exec() );
	}

}
