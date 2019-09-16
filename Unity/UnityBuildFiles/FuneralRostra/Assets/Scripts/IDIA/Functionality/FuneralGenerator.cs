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
public class FuneralGenerator : MonoBehaviour {

	public TextAsset conservativeFileToParse, moderateFileToParse, aggressiveFileToParse;
	public GameObject conservative, moderate, aggressive;
	public Vector3 baseValues;
	public GameObject togaPicta, togaPraetexta, togaPurpurea, bierNone, bierPicta, bierPraetexta, bierPurpurea, text3D;
	//public GameObject[] allFunerals;
	public int currentFuneral=925;
	
	float xDiff = 1f;
	float xPos = 0;
	float baseX = 0;
	float baseZ = 0;
	float zPos = 0;
	float zDiff = -0.2032f;
	float baseZDiff = -0.2032f;
	float zDiffDiff = 0.0762f;
	
	void generateVisualization() {
		for (int fileCount = 1; fileCount < 3; fileCount++) {
			Debug.Log(currentFuneral);
			currentFuneral = 0;
			TextAsset fileToParse = null;
			string outputFileName = null;
			GameObject objectToUse = null;
			if (fileCount == 0) {
				fileToParse = conservativeFileToParse;
				objectToUse = conservative;
				outputFileName = "Assets/ConservativeTextTest.prefab";
			} else if (fileCount == 1) {
				fileToParse = moderateFileToParse;
				objectToUse = moderate;
				outputFileName = "Assets/ModerateTextTest.prefab";
			} else {
				fileToParse = aggressiveFileToParse;
				objectToUse = aggressive;
				outputFileName = "Assets/AggressiveTextTest.prefab";
			}
			string text = fileToParse.text;
			string[] splitString = text.Split('\n');
			
			int eraEnd = -475;
			xPos = baseX;
			
			int deceasedID = 0;
			int funeralIndex = 0;
			string textToDisplay = "";
			for (int lineCount = 0; lineCount < splitString.Length; lineCount++) {
				string thisLine = splitString[lineCount].Trim();     
				thisLine = Regex.Replace(thisLine, @"\s+", " ");

				if (lineCount % 4 == 0) {
					if (thisLine.Equals ("AEMI1134 L. Aemilius (114) L. f. M. n. Paullus Macedonicus -160")) {
						Debug.Log (currentFuneral);
					}
					string[] info = thisLine.Split (' ');
					textToDisplay = thisLine;
					int deathDate = int.Parse (info [info.Length - 1]);
					if (deathDate > 25) {
						break;
					}
					deceasedID = int.Parse (info [0].Substring (info [0].Length - 4, 4));
				} else if (lineCount % 4 == 1) {
					GameObject thisFuneral = new GameObject ("Funeral");
					thisFuneral.transform.SetParent (objectToUse.transform);
					string[] positions = thisLine.Split (' ');
					int endCount = 0;
					if (positions.Length != 0 && (thisLine.Length == 0 || splitString [lineCount] [0] != ' ')) {
						endCount = 2;
					}
					GameObject bier = null;
					int urlPos = 1;
					Vector3 bierPos = new Vector3 (-8f, 19.5f, 115.15f);
					if (positions.Length != 0 || (thisLine.Length == 0 || splitString [lineCount] [0] == ' ')) {
						bier = Instantiate (bierNone, bierPos, Quaternion.Euler (-90, 0, 0));
						urlPos = 0;
					} else if (positions [0].Equals ("triumphator")) {
						bier = Instantiate (bierPicta, bierPos, Quaternion.Euler (-90, 0, 0));
					} else if (positions [0].Equals ("consul")) {
						bier = Instantiate (bierPraetexta, bierPos, Quaternion.Euler (-90, 0, 0));
					} else if (positions [0].Equals ("censor")) {
						bier = Instantiate (bierPurpurea, bierPos, Quaternion.Euler (-90, 0, 0));
					} else if (positions [0].Equals ("praetor")) {
						bier = Instantiate (bierPraetexta, bierPos, Quaternion.Euler (-90, 0, 0));
					}
					bier.GetComponent<WebLinkFuneral> ().url = "" + deceasedID;
					bier.transform.SetParent (thisFuneral.transform);
					
					xPos += 3;

					for (int i = positions.Length - 2; i >= endCount; i -= 2) {
						GameObject toga = null;
						Vector3 togaPos = baseValues + new Vector3 (xPos, 0.0f, zPos);
						if (positions [i].Equals ("triumphator")) {
							toga = Instantiate (togaPicta, togaPos, Quaternion.identity);
						} else if (positions [i].Equals ("consul")) {
							toga = Instantiate (togaPraetexta, togaPos, Quaternion.identity);
						} else if (positions [i].Equals ("censor")) {
							toga = Instantiate (togaPurpurea, togaPos, Quaternion.identity);
						} else if (positions [i].Equals ("praetor")) {
							toga = Instantiate (togaPraetexta, togaPos, Quaternion.identity);
						}
						toga.GetComponent<WebLinkFuneral> ().url = positions [i + 1];
						toga.transform.SetParent (thisFuneral.transform);
						xPos += xDiff;
						zPos += zDiff;
						zDiff += zDiffDiff;
					}
					xPos = baseX;
					zPos = baseZ;
					zDiff = baseZDiff;
					GameObject textObjFinal = Instantiate (text3D, baseValues - new Vector3 (0.0f, 0.0f, 4.0f), Quaternion.identity);
					textObjFinal.GetComponent<TextMesh> ().text = textToDisplay;
					textObjFinal.transform.SetParent (thisFuneral.transform);
					thisFuneral.SetActive (false);
					objectToUse.GetComponent<FuneralControl> ().allFunerals [currentFuneral] = thisFuneral;
				} else if (lineCount % 4 == 2) {
					GameObject toEdit = null;
					objectToUse.GetComponent<FuneralControl> ().treeURLs [currentFuneral] = thisLine;
					currentFuneral++;
				}
			}
			
			//Object prefab = PrefabUtility.CreatePrefab(outputFileName, objectToUse);
		}
	}
	
	void Awake () {
		//generateVisualization();
	}
}
