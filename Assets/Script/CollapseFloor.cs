using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapseFloor : MonoBehaviour
{
	public ExplosionSource expSource;

	public float force = 0f;

	public float startScale = 0;
	public float endScale = 100f;
	private float currentScale;

	public float gainScaleRate = 0.1f;


	public FracturedObject fractuedObject;
	public float upperForce = 15f;

	// Use this for initialization
	void Start () {
		currentScale = startScale;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayEvent()
	{
		expSource.enabled = true;
		expSource.Force = force;
		StartCoroutine( ExecEvent() );
	//	StartCoroutine( AscendChunks() );
	}

	private IEnumerator ExecEvent()
	{
		var diff = endScale - startScale;

		while( currentScale < endScale )
		{
			currentScale += diff * gainScaleRate * Time.deltaTime;

			expSource.InfluenceRadius = currentScale;

			yield return null;
		}

		Debug.Log("CollapseFloor::ExecEvent End");
	}

	private IEnumerator AscendChunks()
	{
		var chunks = fractuedObject.ListFracturedChunks;
		chunks.ForEach( o => 
			o.gameObject.AddComponent<UpperMove>()
		 );

		bool pushAll = false;
		while(!pushAll)
		{
			pushAll = chunks.Count > 0;
			foreach( FracturedChunk o in chunks){
				var rb = o.GetComponent<Rigidbody>();
				if( rb.isKinematic )
				{
					pushAll = false;
					continue;
				}
				var um = rb.GetComponent<UpperMove>();
				if( !um.pushed )
				{
					pushAll = false;
					rb.AddForce( Vector3.up * upperForce );
					um.pushed = true;
				}

			}
			yield return null;
		}
		Debug.Log("CollapseFloor::AscendChunks End");

	}
}

public class UpperMove : MonoBehaviour
{
	public bool pushed = false;
}