using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTest : MonoBehaviour 
{
	private bool falling = false;
	public float fallingSpeedMax = 1f;
	public float fallingSpeedMin = 1f;
	public float interval = 1f;
	public bool enableLoop = false;
	public void SetIsFalling()
	{
		SetIsFalling(!falling);
	}
	public void SetIsFalling(bool value)
	{
		falling = value;
	}

	private float originHeight = 0;
	public float loopThresholdHeight = 20f;
	public void StartFalling()
	{
		originHeight = transform.position.y;
		SetIsFalling( true );
	}

	void Update()
	{
		// 落下している感じを出そうと頑張っているけど未だできていない
		{
			if( falling )
			{			
				if( enableLoop && Mathf.Abs( transform.position.y ) > loopThresholdHeight )
				{
					transform.position = new Vector3( transform.position.x, originHeight, transform.position.z);
				}

				var speedDif = fallingSpeedMax - fallingSpeedMin;
				if( speedDif == 0 ) speedDif = 1;

				var speed = fallingSpeedMin + Mathf.PingPong(Time.time * interval, speedDif);
				transform.position += Vector3.down * speed * Time.deltaTime;
			}
		}

	}
}
