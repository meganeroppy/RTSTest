using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AngryEffect : MonoBehaviour {

	[SerializeField]
	float punch = 0.2f;

	[SerializeField]
	float duration = 1f;

	[SerializeField]
	float remain = 1f;

	void Start()
	{
		StartCoroutine( Exec() ); 
	}

	// Update is called once per frame
	IEnumerator Exec () 
	{
		var origin = transform.position;

		bool complete;
		complete = false;
		transform.DOMoveY(punch, duration * 0.5f).SetEase(Ease.OutCubic).OnComplete( () => { complete = true; } );
		while( !complete ) yield return null;

		complete = false;
		transform.DOMoveY(origin.y, duration * 0.5f).SetEase(Ease.OutBounce).OnComplete( () => { complete = true; } );
		while( !complete ) yield return null;

		yield return new WaitForSeconds(remain );

		Destroy(gameObject);
	}
}
