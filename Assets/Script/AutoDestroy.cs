using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour {

    ParticleSystem ps;

    float lifeTime;
    float timer = 0;

	// Use this for initialization
	void Start ()
    {
        ps = GetComponent<ParticleSystem>();

        lifeTime = ps.main.duration;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if( timer >= lifeTime )
        {
            Destroy(this.gameObject);
        }
    }
}
