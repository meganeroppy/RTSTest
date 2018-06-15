using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiraiBoyController : MonoBehaviour {
	[SerializeField]
	IKControl ik;

	void Start()
	{
		StartCoroutine (OnLoad ());
	}

	IEnumerator OnLoad()
	{
		GameObject obj;
		bool first;

		{
			obj = null;
			first = true;
			do {
				if (!first)
					yield return new WaitForSeconds (1);

				first = false;
				obj = GameObject.Find ("HiraiHead(Clone)");
			} while(!obj);

			ik.lookObj = obj.transform.GetChild (0).transform;
		}

		{
			obj = null;
			first = true;
			do {
				if (!first)
					yield return new WaitForSeconds (1);
				first = false;
				obj = GameObject.Find ("HiraiRH(Clone)");
			} while(!obj);

			ik.rightHandObj = obj.transform;
		}

		{
			obj = null;
			first = true;
			do {
				if (!first)
					yield return new WaitForSeconds (1);
				first = false;
				obj = GameObject.Find ("HiraiLH(Clone)");
			} while(!obj);

			ik.leftHandObj = obj.transform;
		}
		{
			obj = null;
			first = true;
			do {
				if (!first)
					yield return new WaitForSeconds (1);
				first = false;
				obj = GameObject.Find ("HiraiRF(Clone)");
			} while(!obj);

			ik.rightFootObj = obj.transform;
		}

		{
			obj = null;
			first = true;
			do {
				if (!first)
					yield return new WaitForSeconds (1);
				first = false;
				obj = GameObject.Find ("HiraiLF(Clone)");
			} while(!obj);

			ik.leftFootObj = obj.transform;
		}
		}

}