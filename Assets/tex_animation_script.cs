using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class tex_animation_script : MonoBehaviour {





	public Texture2D TEX_01;
	public Texture2D TEX_02;
	public Texture2D TEX_03;

	public bool TEX_01_ON=true;
	public bool TEX_02_ON=false;
	public bool TEX_03_ON=false;
	void Update(){
		if (TEX_01_ON) {
			gameObject.GetComponent<Renderer> ().material.mainTexture = TEX_01;
			TEX_02_ON = false;
			TEX_03_ON = false;
			TEX_01_ON = false;
		}
		if (TEX_02_ON) {
			gameObject.GetComponent<Renderer> ().material.mainTexture = TEX_02;
			TEX_02_ON = false;
			TEX_03_ON = false;
			TEX_01_ON = false;
		}
		if(TEX_03_ON) {
			gameObject.GetComponent<Renderer> ().material.mainTexture = TEX_03;
			TEX_02_ON = false;
			TEX_03_ON = false;
			TEX_01_ON = false;
		}
	}



}
