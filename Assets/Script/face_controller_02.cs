using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class face_controller_02 : MonoBehaviour {
	public bool face_controll;

	public GameObject face;


	public Texture2D eye_tex_01;
	public Texture2D eye_tex_02;
	public Texture2D eye_tex_03;
	public Texture2D eye_tex_04;

	bool eye_tex_01_flag=true;
	bool eye_tex_02_flag=false;
	bool eye_tex_03_flag=false;
	bool eye_tex_04_flag=false;

	public Texture2D mouth_tex_01;
	public Texture2D mouth_tex_02;
	public Texture2D mouth_tex_03;
	public Texture2D mouth_tex_04;

	bool mouth_tex_01_flag=true;
	bool mouth_tex_02_flag=false;
	bool mouth_tex_03_flag=false;
	bool mouth_tex_04_flag=false;

	bool eye_controll = false;
	bool mouth_controll = false;
	bool hand_controll = false;


	public Animator animator;

	void Start() {

	}

	void Update() {
		if (face_controll) {


			if (Input.GetKeyDown (KeyCode.JoystickButton5)) {
				eye_controll = true;
			}

			if (Input.GetKeyUp (KeyCode.JoystickButton5)) {
				eye_controll = false;
			}

			if (Input.GetKeyDown (KeyCode.JoystickButton4)) {
				mouth_controll = true;
			}

			if (Input.GetKeyUp (KeyCode.JoystickButton4)) {
				mouth_controll = false;

			}

			if (Input.GetKeyDown (KeyCode.JoystickButton6)) {
				hand_controll = true;
			}

			if (Input.GetKeyUp (KeyCode.JoystickButton6)) {
				hand_controll = false;

			}
			if (eye_controll) {
				if (Input.GetKeyDown (KeyCode.JoystickButton0)) {
					ReplaceMaterial (2, eye_tex_01);
					eye_tex_01_flag = false;
					eye_tex_02_flag = false;
					eye_tex_03_flag = false;
					eye_tex_04_flag = false;
				}
				if (Input.GetKeyDown (KeyCode.JoystickButton1)) {
					ReplaceMaterial (2, eye_tex_02);
					eye_tex_01_flag = false;
					eye_tex_02_flag = false;
					eye_tex_03_flag = false;
					eye_tex_04_flag = false;
				}
				if (Input.GetKeyDown (KeyCode.JoystickButton2)) {
					ReplaceMaterial (2, eye_tex_03);
					eye_tex_01_flag = false;
					eye_tex_02_flag = false;
					eye_tex_03_flag = false;
					eye_tex_04_flag = false;
				}
				if (Input.GetKey (KeyCode.JoystickButton3)) {
					ReplaceMaterial (2, eye_tex_04);
					eye_tex_01_flag = false;
					eye_tex_02_flag = false;
					eye_tex_03_flag = false;
					eye_tex_04_flag = false;
				}
			}

			if (mouth_controll) {
				if (Input.GetKeyDown (KeyCode.JoystickButton0)) {
					ReplaceMaterial (1, mouth_tex_01);
					mouth_tex_01_flag = false;
					mouth_tex_02_flag = false;
					mouth_tex_03_flag = false;
					mouth_tex_04_flag = false;
				}
				if (Input.GetKeyDown (KeyCode.JoystickButton1)) {
					ReplaceMaterial (1, mouth_tex_02);
					mouth_tex_01_flag = false;
					mouth_tex_02_flag = false;
					mouth_tex_03_flag = false;
					mouth_tex_04_flag = false;
				}
				if (Input.GetKeyDown (KeyCode.JoystickButton2)) {
					ReplaceMaterial (1, mouth_tex_03);
					mouth_tex_01_flag = false;
					mouth_tex_02_flag = false;
					mouth_tex_03_flag = false;
					mouth_tex_04_flag = false;
				}
				if (Input.GetKey (KeyCode.JoystickButton3)) {
					ReplaceMaterial (1, mouth_tex_04);
					mouth_tex_01_flag = false;
					mouth_tex_02_flag = false;
					mouth_tex_03_flag = false;
					mouth_tex_04_flag = false;
				}
			}

			if (hand_controll) {
				if (Input.GetKeyDown (KeyCode.JoystickButton0)) {
					animator.SetBool ("guu", true);
					animator.SetBool ("paa", false);

				}
				if (Input.GetKeyDown (KeyCode.JoystickButton1)) {
					animator.SetBool ("guu", false);
					animator.SetBool ("paa", true);
				}
				if (Input.GetKeyDown (KeyCode.JoystickButton2)) {
					animator.SetBool ("guu", true);
					animator.SetBool ("paa", true);
				}
				if (Input.GetKey (KeyCode.JoystickButton3)) {
					animator.SetBool ("guu", false);
					animator.SetBool ("paa", false);
				}
			}
		}
	}



	private void ReplaceMaterial(int index, Texture tex)

	{

		Renderer renderer = face.GetComponent<Renderer>();

		Material[] mats = renderer.materials;

		if (index < 0 || mats.Length <= index) return;

		mats[index].mainTexture = tex;

		renderer.materials = mats;

	}
		




}
