using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateWalk : MonoBehaviour
{
	public Transform[] feet;

	public float groundHeight = 0;
	public float height = 1f;
	public float speed = 0.1f;
	public float openWidth = 0.5f;
	public Transform root;

	private int idx = 0;

	// Use this for initialization
	void Start ()
	{
		if( height > 0 )
			StartCoroutine( Exec() );
	}

	void Update()
	{
		foreach( Transform foot in feet )
		{
			foot.forward = root.forward;
	//		foot.rotation = Quaternion.Euler( 0, foot.rotation.y, foot.rotation.z );

			var origin = foot.localPosition;

			var diff = openWidth * 0.5f;
			foot.localPosition = root.localPosition + diff * (foot.name.Contains("Right") ? root.right : -root.right);

			foot.localPosition = new Vector3( foot.localPosition.x, origin.y, origin.z );
		}
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
