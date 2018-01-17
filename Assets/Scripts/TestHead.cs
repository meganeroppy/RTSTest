using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHead : Photon.MonoBehaviour {

    public float speed = 50f;

    private void Start()
    {
        if (!photonView.isMine)
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
        if (photonView.isMine)
        {
            var move_x = Input.GetAxis("Horizontal");
            var move_z = Input.GetAxis("Vertical");

            var add = new Vector3(move_x, 0, move_z);

            transform.position += add * speed * Time.deltaTime;
        }
    }
}
