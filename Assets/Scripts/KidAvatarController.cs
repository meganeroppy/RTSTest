using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// こどもアバターの制御
/// </summary>
public class KidAvatarController : AvatarController
{
	private List<GameObject> kidsList;

	private Transform objectRoot;

	public int testIndex = 0;
	protected override void Init ()
	{
		kidsList = new List<GameObject>();

		for( int i = 0 ; i < transform.childCount ; i++ )
		{
			kidsList.Add( transform.GetChild( i ).gameObject);
		}

		//仮
		SetActiveKid( testIndex );
	}

	public void SetActiveKid( int index )
	{
		// 指定インデックス以外削除
		index %= kidsList.Count;
		for( int i = kidsList.Count-1 ; i >= 0 ; --i )
		{
			if( i != index )
			{
				Destroy( kidsList[i] );
			}
		}
			
		var kid = kidsList[ 0 ];

		kid.gameObject.SetActive( true );

		objectRoot = kid.transform;

		modelRoot = transform;

		anim = kid.GetComponent<Animator>();
	}
	/*
	protected override void UpdatePositionAndRotation()
	{
		if( objectRoot == null ) return;

		objectRoot.SetPositionAndRotation(ownerPosition, ownerRotation);
	}
	*/
}
