using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoyEffect : MonoBehaviour {

	[SerializeField]
	float interval = 0.2f;

	float timer = 0;

	[SerializeField]
	int loopNumber = 4;

	[SerializeField]
	float remain = 1f;

	void Start()
	{
		StartCoroutine( StartLoop() ); 
	}

	// Update is called once per frame
	IEnumerator StartLoop () {
		int loop = 0;
		Vector3 scale;

		while( loop < loopNumber )
		{
			yield return new WaitForSeconds( interval );
			scale = transform.localScale;
			scale.x *= -1f;
			transform.localScale = scale;

			yield return new WaitForSeconds( interval );
			scale = transform.localScale;
			scale.x *= -1f;
			transform.localScale = scale;

			loop++;
		}

		yield return new WaitForSeconds(remain );

		Destroy(gameObject);
	}
}
