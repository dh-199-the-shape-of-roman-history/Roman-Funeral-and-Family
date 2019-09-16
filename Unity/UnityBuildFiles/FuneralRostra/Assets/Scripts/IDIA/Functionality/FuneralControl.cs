using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/**
 * This script causes any object with it attached to turn to face the main camera.  This is useful for 2-dimensional text.
 * Author: Benjamin Niedzielski (bniedzie@ucla.edu)
 */
public class FuneralControl : MonoBehaviour {

	public GameObject[] allFunerals;
	public string[] treeURLs;
	//public int currentFuneral = 695;
	
	void Update() {
		int currentFuneral = GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral;
		if (Input.GetKeyDown(KeyCode.J)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			if (GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral > 0)
				GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral--;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
		if (Input.GetKeyDown(KeyCode.K)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			if(GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral < allFunerals.Length - 1)
				GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral++;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
		if (Input.GetKey(KeyCode.N)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			if (GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral > 0)
				GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral--;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
		if (Input.GetKey(KeyCode.M)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			if(GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral < allFunerals.Length - 1)
				GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral++;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
		if (Input.GetKeyDown(KeyCode.U)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral = 0;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
		if (Input.GetKeyDown(KeyCode.I)) {
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(false);
			GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral = 925;
			allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
		}
	}
	
	void FixedUpdate() {
		for(int i = 0; i < allFunerals.Length; i++) {
			allFunerals[i].SetActive(false);
		}
		//Debug.Log (GameObject.Find ("Scene Objects").GetComponent<FuneralGenerator> ().currentFuneral);
		allFunerals[GameObject.Find("Scene Objects").GetComponent<FuneralGenerator>().currentFuneral].SetActive(true);
	}
}
