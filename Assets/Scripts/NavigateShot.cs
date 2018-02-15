using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ナビゲーターがプレイヤーをナビするときに使うショット
/// </summary>
public class NavigateShot : NetworkBehaviour {

	[SerializeField]
	private GameObject effect = null;

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

			Dead();
		}
	}

    [ServerCallback]
	void OnTriggerEnter( Collider col )
	{
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod() + " : " + col.name);

		if( dead ) return;

        // ターゲットだったらヒット
        if( col.tag == "Item" )
        {
            var enemy = col.GetComponent<ShootTarget>();
            if( enemy != null )
            {
                enemy.Hit();
            }
        }

		dead = true;

		Dead();
	}

	[Server]
	private void Dead()
	{
		NetworkServer.Destroy( gameObject );
	}

	/// <summary>
	/// サーバーでもクライアントでも呼ばれる
	/// </summary>
	private void OnDestroy()
	{
		Instantiate(effect, transform.position, Quaternion.identity);
	}
}
