using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class face_controller : MonoBehaviour {
	public bool face_controll;
	public GameObject eye_left;
	public GameObject eye_right;
	public GameObject eyelid_left;
	public GameObject eyelid_right;
	public GameObject face;

	float h;
	float v;

	float h2;
	float v2;

	float v3;
	float v4;

	bool lid_right;
	bool lid_left;

	public Texture2D TEX_01;
	public Texture2D TEX_02;
	public Texture2D TEX_03;

	bool TEX_01_ON=true;
	bool TEX_02_ON=false;
	bool TEX_03_ON=false;

	public bool eye_right_reverce=false;
	public bool eye_left_reverce=false;

	void Start() {
		//GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", Vector2.zero);
		h = 0.15f;
		h2 = 0.15f;
		v = 0;
		v2 = 0;
		v3 = 0.5f;
		v4 = 0.5f;

		var offset = new Vector2(h, v);
		if (eye_left_reverce) {
			h = 0;

			offset = new Vector2 (-h, v);
		} 
		eye_left.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);
		var offset2 = new Vector2(h2, v2);
		if (eye_right_reverce) {
			h2 = 0;
			offset2 = new Vector2 (-h2, v2);
		}
		eye_right.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset2);

		var offset3 = new Vector2 (v3, 0);
		eyelid_left.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset3);
		var offset4 = new Vector2 (v4, 0);
		eyelid_right.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset4);
	}

	void Update() {
		if (face_controll) {


			if (Input.GetKeyDown (KeyCode.JoystickButton5)) {
				lid_right = true;
			}

			if (Input.GetKeyUp (KeyCode.JoystickButton5)) {
				lid_right = false;
			}

			if (Input.GetKeyDown (KeyCode.JoystickButton4)) {
				lid_left = true;
			}

			if (Input.GetKeyUp (KeyCode.JoystickButton4)) {
				lid_left = false;

			}
			if (Input.GetKeyDown (KeyCode.JoystickButton0)) {
				face.GetComponent<Renderer> ().material.mainTexture = TEX_01;
				TEX_01_ON = false;
				TEX_02_ON = false;
				TEX_03_ON = false;

			}
			if (Input.GetKeyDown (KeyCode.JoystickButton1)) {
				face.GetComponent<Renderer> ().material.mainTexture = TEX_02;
				TEX_01_ON = false;
				TEX_02_ON = false;
				TEX_03_ON = false;
			}
			if (Input.GetKeyDown (KeyCode.JoystickButton2)) {
				face.GetComponent<Renderer> ().material.mainTexture = TEX_03;
				TEX_01_ON = false;
				TEX_02_ON = false;
				TEX_03_ON = false;
			}
			if (Input.GetKey (KeyCode.JoystickButton3)) {
				h = 0.15f;
				v = 0;
				var offset = new Vector2 (h, v);
				if (eye_left_reverce) {
					h = 0;

					offset = new Vector2 (-h, v);
				} 
				eye_left.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset);
				h2 = 0.15f;
				v2 = 0;
				var offset2 = new Vector2 (h2, v2);
				if (eye_right_reverce) {
					h2 = 0;
					offset2 = new Vector2 (-h2, v2);
				}

				eye_right.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset2);
				v3 = 0.5f;
				v4 = 0.5f;
				var offset3 = new Vector2 (v3, 0);
				eyelid_left.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset3);
				var offset4 = new Vector2 (v4, 0);
				eyelid_right.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset4);
			}

			if (!(lid_right || lid_left)) {
				if (Input.GetAxis ("Horizontal2") > 0.2f) {
					h -= 0.02f;
				}
				if (Input.GetAxis ("Horizontal2") < -0.2f) {
					h += 0.02f;
				}
				if (Input.GetAxis ("Vertical2") > 0.2f) {
					v -= 0.02f;
				}
				if (Input.GetAxis ("Vertical2") < -0.2f) {
					v += 0.02f;
				}


				if (eye_left_reverce) {
					h = Math.Min (Math.Max (-0.25f, h), 0.1f);
					v = Math.Min (Math.Max (-0.2f, v), 0);
				} else {
					h = Math.Min (Math.Max (-0.1f, h), 0.25f);
					v = Math.Min (Math.Max (-0.2f, v), 0);
				}



				var offset = new Vector2 (h, v);
				if (eye_left_reverce) {
					 offset = new Vector2 (-h, v);
				} 



				eye_left.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset);

				if (Input.GetAxis ("Horizontal") > 0.2f) {
					h2 += 0.02f;
				}
				if (Input.GetAxis ("Horizontal") < -0.2f) {
					h2 -= 0.02f;
				}
				if (Input.GetAxis ("Vertical") > 0.2f) {
					v2 -= 0.02f;
				}
				if (Input.GetAxis ("Vertical") < -0.2f) {
					v2 += 0.02f;
				}

				if (eye_right_reverce) {
					h2 = Math.Min (Math.Max (-0.25f, h2), 0.1f);
					v2 = Math.Min (Math.Max (-0.2f, v2), 0);
				} else {
					h2 = Math.Min (Math.Max (-0.1f, h2), 0.25f);
					v2 = Math.Min (Math.Max (-0.2f, v2), 0);
				}

				var offset2 = new Vector2 (h2, v2);
				if (eye_right_reverce) {
					 offset2 = new Vector2 (-h2, v2);
				}

				eye_right.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset2);
			}
			if (lid_right && !lid_left) {
			
				if (Input.GetAxis ("Vertical2") > 0.8f) {
					v3 += 0.04f;
				} else if (Input.GetAxis ("Vertical2") > 0.4f) {
					v3 += 0.02f;
				} else if (Input.GetAxis ("Vertical2") > 0.2f) {
					v3 += 0.01f;
				}

				if (Input.GetAxis ("Vertical2") < -0.8f) {
					v3 -= 0.04f;
				} else if (Input.GetAxis ("Vertical2") < -0.4f) {
					v3 -= 0.02f;
				} else if (Input.GetAxis ("Vertical2") < -0.2f) {
					v3 -= 0.01f;
				}


				v3 = Math.Min (Math.Max (0, v3), 0.5f);

				var offset3 = new Vector2 (v3, 0);

				eyelid_left.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset3);

			}
			if (lid_left && !lid_right) {


				if (Input.GetAxis ("Vertical") > 0.8f) {
					v4 += 0.04f;
				} else if (Input.GetAxis ("Vertical") > 0.4f) {
					v4 += 0.02f;
				} else if (Input.GetAxis ("Vertical") > 0.2f) {
					v4 += 0.01f;
				}

				if (Input.GetAxis ("Vertical") < -0.8f) {
					v4 -= 0.04f;
				} else if (Input.GetAxis ("Vertical") < -0.4f) {
					v4 -= 0.02f;
				} else if (Input.GetAxis ("Vertical") < -0.2f) {
					v4 -= 0.01f;
				}


				v4 = Math.Min (Math.Max (0, v4), 0.5f);

				var offset4 = new Vector2 (v4, 0);

				eyelid_right.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset4);

			}
			if (lid_left && lid_right) {
				if (Input.GetAxis ("Vertical2") > 0.8f) {
					v3 += 0.04f;
				} else if (Input.GetAxis ("Vertical2") > 0.4f) {
					v3 += 0.02f;
				} else if (Input.GetAxis ("Vertical2") > 0.2f) {
					v3 += 0.01f;
				}
				if (Input.GetAxis ("Vertical2") < -0.8f) {
					v3 -= 0.04f;
				} else if (Input.GetAxis ("Vertical2") < -0.4f) {
					v3 -= 0.02f;
				} else if (Input.GetAxis ("Vertical2") < -0.2f) {
					v3 -= 0.01f;
				}


				v3 = Math.Min (Math.Max (0, v3), 0.5f);

				var offset3 = new Vector2 (v3, 0);

				eyelid_left.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset3);

				if (Input.GetAxis ("Vertical") > 0.8f) {
					v3 += 0.04f;
				} else if (Input.GetAxis ("Vertical") > 0.4f) {
					v3 += 0.02f;
				} else if (Input.GetAxis ("Vertical") > 0.2f) {
					v3 += 0.01f;
				}

				if (Input.GetAxis ("Vertical") < -0.8f) {
					v3 -= 0.04f;
				} else if (Input.GetAxis ("Vertical") < -0.4f) {
					v3 -= 0.02f;
				} else if (Input.GetAxis ("Vertical") < -0.2f) {
					v3 -= 0.01f;
				}


				v3 = Math.Min (Math.Max (0, v3), 0.5f);

				var offset4 = new Vector2 (v3, 0);

				eyelid_right.GetComponent<Renderer> ().sharedMaterial.SetTextureOffset ("_MainTex", offset4);
				v4 = v3;
			}



		}
	}






}
