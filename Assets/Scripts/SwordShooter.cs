using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SwordShooter : NetworkBehaviour
{
	[SerializeField]
	private Transform shotOriginRight;
	[SerializeField]
	private Transform shotOriginLeft;

	[SerializeField]
	private SwordShot shot;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (OVRInput.GetDown(OVRInput.RawButton.A)) {
			Debug.Log("Aボタンを押した");
		}
		if (OVRInput.GetDown(OVRInput.RawButton.B)) {
			Debug.Log("Bボタンを押した");
		}
		if (OVRInput.GetDown(OVRInput.RawButton.X)) {
			Debug.Log("Xボタンを押した");
		}
		if (OVRInput.GetDown(OVRInput.RawButton.Y)) {
			Debug.Log("Yボタンを押した");
		}
		if (OVRInput.GetDown(OVRInput.RawButton.Start)) {
			Debug.Log("メニューボタン（左アナログスティックの下にある）を押した");
		}

		if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) {
			Debug.Log("右人差し指トリガーを押した");
			Shot( shotOriginRight );
		}
		if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)) {
			Debug.Log("右中指トリガーを押した");
		}
		if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger)) {
			Debug.Log("左人差し指トリガーを押した");
			Shot( shotOriginLeft );
		}
		if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger)) {
			Debug.Log("左中指トリガーを押した");
		}	
	}

	private void Shot(Transform shotOrigin)
	{
        if ( isLocalPlayer )
    //        if (photonView != null && photonView.isMine)
        {
        //        PhotonNetwork.Instantiate( shot.name, shotOrigin.position, shotOrigin.rotation, 0);
		}
		else
		{
		//	Instantiate( shot, shotOrigin.position, shotOrigin.rotation);
		}
	}
}
