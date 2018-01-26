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
	private Transform[] walls = null;

	[SerializeField]
	private float wallScale = 10f;

	[SerializeField]
	private float moveSpeed = 10f;

	private float offset;

	// Use this for initialization
	void Start ()
	{
		// 初期位置をセット
		if( walls.Length != 2 )
		{
			Debug.LogError("壁の数は２枚の前提");
		}

		int index = 0;
		offset = wallScale * 2f;
		foreach( Transform w in walls )
		{
			w.localScale = Vector3.one * wallScale;
			w.localPosition = (wallBase.position - Vector3.up * wallScale ) + ( Vector3.up * offset * index++ );
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		// 壁の位置更新
		foreach( Transform w in walls )
		{
			w.Translate(Vector3.up * moveSpeed * Time.deltaTime);
			if( w.localPosition.y >= wallScale  )
			{
				// 限界になったら位置を循環させる
				w.Translate( Vector3.down * offset * 2); 
			}
		}
	}
}
