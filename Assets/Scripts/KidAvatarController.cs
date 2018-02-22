using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// こどもアバターの制御
/// </summary>
public class KidAvatarController : AvatarController
{
	private List<IKControl> kidsList;

	private Transform objectRoot;

	private void Start()
	{
		kidsList = new List<IKControl>();

		for( int i = 0 ; i < transform.childCount ; i++ )
		{
			kidsList.Add( transform.GetChild( i ).GetComponent<IKControl>());
		}

		SetActiveKid( 0 );
	}

	public void SetActiveKid( int index )
	{
		// いったんすべて無効
		foreach( IKControl k in kidsList )
		{
			k.gameObject.SetActive( false );
		}

		index %= kidsList.Count;

		var kid = kidsList[ index ];

		kid.gameObject.SetActive( true );

		objectRoot = kid.transform;

		var mRoot = kid.transform.Find("ROOT");
		if( !mRoot ) 
		{
			Debug.LogError(" [ROOT] がみつからない");
			return;
		}

		modelRoot = mRoot;
	}

	protected override void UpdatePositionAndRotation()
	{
		if( objectRoot == null ) return;

		objectRoot.SetPositionAndRotation(ownerPosition, ownerRotation);
	}
}
