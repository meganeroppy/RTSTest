using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestHead : NetworkBehaviour {

    public float speed = 50f;

    private void Start()
    {
    //    if (!photonView.isMine)
            if (!isLocalPlayer)
            {
                var camera = GetComponent<Camera>();
            if (camera != null) Destroy(camera);
        }
    }
    // Update is called once per frame
    void Update () {
        UpdatePositioin();
	}

    void UpdatePositioin()
    {
    //    if (photonView.isMine)
            if (isLocalPlayer)
            {
                var move_x = Input.GetAxis("Horizontal");
            var move_z = Input.GetAxis("Vertical");

            var add = new Vector3(move_x, 0, move_z);

            transform.position += add * speed * Time.deltaTime;
        }
    }
}
