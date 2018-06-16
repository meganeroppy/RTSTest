using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class target_controller : MonoBehaviour {
	public GameObject[] chara_arr = new GameObject[6];
	public bool[] chara_enable = new bool[6];
	public int target_cnt=0;
	public int go_cnt;
	void Update () {
		for (int i = 0; i < 6; i++) {
			if (chara_enable [i]) {
				chara_arr [i].GetComponent<face_controller> ().face_controll = true;
			} else {
				chara_arr [i].GetComponent<face_controller> ().face_controll = false;
			}
		}


		if (go_cnt == 0 && Input.GetAxis ("Axis 6") > 0.2f) {
			target_cnt++;
			target_cnt %=6;
			go_cnt = 5;

		}else if (go_cnt == 0 && Input.GetAxis ("Axis 6") < -0.2f) {
			target_cnt--;
			target_cnt += 6;
			target_cnt %=6;
			go_cnt = 5;
		} else{
			go_cnt --;
			go_cnt = Mathf.Max (go_cnt,0);

		}

		if (Input.GetAxis ("Axis 7") > 0.2f) {
			chara_arr [target_cnt].GetComponent<face_controller> ().face_controll = true;
			chara_enable [target_cnt] = true;

		}
		if (Input.GetAxis ("Axis 7") < -0.2f) {
			chara_arr [target_cnt].GetComponent<face_controller> ().face_controll = false;
			chara_enable [target_cnt] = false;


		} 
		
	}
}
