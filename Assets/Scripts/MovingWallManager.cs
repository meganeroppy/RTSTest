using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 落ちていく穴を再現
/// </summary>
public class MovingWallManager : MonoBehaviour 
{
	[SerializeField]
	private Transform wallBase = null;

	/// <summary>
	/// 壁
	/// ２枚の前提
	/// </summary>
	[SerializeField]
	private Transform wall = null;

	[SerializeField]
	private float wallTall = 10f;

	[SerializeField]
	private float wallRadius = 10f;

	[SerializeField]
	private float moveSpeed = 10f;

	private float flapLate = 0.5f;

	[SerializeField]
	private Transform topWall;

	[SerializeField]
	private Transform bottomWall;

	// Use this for initialization
	void Start ()
	{
		var flapLimit = wallTall * flapLate;

		wall.localScale = new Vector3( wallRadius, wallTall, wallRadius);
		wall.localPosition = wallBase.position - Vector3.up * flapLimit;

		topWall.localScale = bottomWall.localScale = new Vector3( wallRadius, 1f, wallRadius );
		topWall.localPosition = Vector3.up * wallTall * flapLate;
		bottomWall.localPosition = Vector3.down * wallTall * flapLate;
	}
	
	// Update is called once per frame
	void Update () 
	{
		var flapLimit = wallTall * flapLate;

		// 壁の位置更新
		wall.Translate(Vector3.up * moveSpeed * Time.deltaTime);
		if( wall.localPosition.y >= flapLimit  )
		{
			// 限界になったら位置を循環させる
			wall.Translate( Vector3.down * flapLimit * 2); 
		}
	}
}
