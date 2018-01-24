using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 観測者がプレイヤーをナビするときに使うショット
/// </summary>
public class NavigateShot : NetworkBehaviour {

	[SerializeField]
	private GameObject effect;

	[SerializeField]
	private float speed = 3f;

	[SerializeField]
	private float lifeTime = 2f;

	private float timer = 0;

	private bool dead = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
    [ServerCallback]
	void Update () 
	{
		if(dead)return;

		transform.position += transform.forward * speed * Time.deltaTime;

		timer += Time.deltaTime;
		if( timer > lifeTime )
		{
			dead = true;

			CreateEffect();
		}
	}

    [ServerCallback]
	void OnCollisionEnter( Collision col )
	{
		if( dead ) return;

		dead = true;

		CreateEffect();
	}

	private void CreateEffect()
	{
		var obj = Instantiate(effect, transform.position, Quaternion.identity, transform);

        NetworkServer.Spawn(obj);

		Destroy(gameObject, 2f);
	}
}
