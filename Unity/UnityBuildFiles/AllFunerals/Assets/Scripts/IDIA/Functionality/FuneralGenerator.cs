using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/**
 * This script generates bar-graph visualizations of all known funerals based on three
 * text files, producing either a full version or a compact version for each.
 * It can save these visualizations as prefabs.
 * Author: Benjamin Niedzielski (bniedzie@ucla.edu)
 */
public class FuneralGenerator : MonoBehaviour {

	//The files to parse
	public TextAsset conservativeFileToParse, moderateFileToParse, aggressiveFileToParse;
	//The starting locations
	public Vector3 baseValues;
	//The assets to copy
	public GameObject togaPicta, togaPraetexta, togaPurpurea, bierNone, bierPicta, bierPraetexta, bierPurpurea, text3D;
	//Options to determine the details of the generated prefabs
	public bool showDeceased, collapseAncestorless;
	
	//Determines the spacing between funerals and ancestors
	float xDiff = 1f;
	float zDiff = 1.2f;
	float zPos = 0;
	float xPos = 0;
	float baseX = 0;
	float baseZ = 0;
	
	//Generate the visualizations and create prefabs for them
	void generateVisualization() {
		for (int fileCount = 0; fileCount < 3; fileCount++) {
			TextAsset fileToParse = null;
			string outputFileName = null;
			//Choose the file to read for this loop
			if (fileCount == 0) {
				fileToParse = conservativeFileToParse;
				if(!collapseAncestorless)
					outputFileName = "Assets/ConservativeTextTest.prefab";
				else
					outputFileName = "Assets/ConservativeTextCollapsedTest.prefab";
			} else if (fileCount == 1) {
				fileToParse = moderateFileToParse;
				if(!collapseAncestorless)
					outputFileName = "Assets/ModerateTextTest.prefab";
				else
					outputFileName = "Assets/ModerateTextCollapsedTest.prefab";
			} else {
				fileToParse = aggressiveFileToParse;
				if(!collapseAncestorless)
					outputFileName = "Assets/AggressiveTextTest.prefab";
				else
					outputFileName = "Assets/AggressiveTextCollapsedTest.prefab";
			}
			string text = fileToParse.text;
			string[] splitString = text.Split('\n');
			
			//Group all funerals prior to 475 BCE together
			int eraEnd = -475;
			baseX = baseValues.x;
			baseZ = baseValues.z;
			xPos = baseX;
			zPos = baseZ;
			float ancestorlessZ = baseZ - zDiff;
			float ancestorlessY = baseValues.y + 2;
			float yDiff = 1.5f;
			
			//Create a parent GameObject to group as a single prefab.
			GameObject parent = new GameObject("Funerals");
			
			//Track the number of funerals in each group for text-based information
			int deceasedID = 0;
			int funeralCount = 0;
			int ancestorFuneralCount = 0;
			//Each funeral takes up 2 lines in the files, with a third blank line between each
			for (int lineCount = 0; lineCount < splitString.Length; lineCount++) {
				string thisLine = splitString[lineCount].Trim();     
				thisLine = Regex.Replace(thisLine, @"\s+", " ");

				if (lineCount % 4 == 0) {
					//The first line contains the deceased's name, number, and death date, which is at the end
					string[] info = thisLine.Split(' ');
					int deathDate = int.Parse(info[info.Length - 1]);
					if (deathDate > eraEnd) {
						//Each column is 25 years, so if the new funeral happens after the current column, start a new one
						//Each column gets descriptive text at this point
						xPos = baseX;
						zPos = baseZ - 3;
						GameObject textObj = Instantiate(text3D, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
						textObj.GetComponent<TextMesh>().text = "" + (eraEnd - 24) + " to " + eraEnd + "\n" + funeralCount + " funerals\n" + ancestorFuneralCount + " with ancestors";
						funeralCount = 0;
						ancestorFuneralCount = 0;
						textObj.transform.SetParent(parent.transform);
						eraEnd = deathDate / 25 * 25;
						if (eraEnd >= 0) {
							//Handle C#'s truncating for positive and negative numbers
							eraEnd += 25;
						}
						zPos = baseZ;
						ancestorlessY = baseValues.y + 2;
						baseX += 15f;
						xPos = baseX;
					}
					deceasedID = int.Parse(info[0].Substring(info[0].Length - 4, 4));
				} else if (lineCount % 4 == 1) {
					//The second line contains a list of ancstor positions and IDs
					GameObject thisFuneral = new GameObject("Funeral");
					thisFuneral.transform.SetParent(parent.transform);
					string[] positions = thisLine.Split(' ');
					//Determine if the first position listed is the deceased or not
					int endCount = 0;
					if (positions.Length != 0 && (thisLine.Length == 0 || splitString[lineCount][0] != ' ')) {
						endCount = 2;
					}
					xPos = baseX;
					if (showDeceased) {
						//Choose the right bier and place it
						GameObject bier = null;
						int urlPos = 1;
						Vector3 bierPos = new Vector3(0.0f, 0.0f, 0.0f);
						if (positions.Length - 2 >= endCount || !collapseAncestorless) {
							bierPos = new Vector3(xPos, baseValues.y + 2, zPos - 10);
						} else {
							bierPos = new Vector3(xPos, ancestorlessY, ancestorlessZ - 10);
							ancestorlessY -= yDiff;
						}
						if (positions.Length != 0 || (thisLine.Length == 0 || splitString[lineCount][0] == ' ')) {
							bier = Instantiate(bierNone, bierPos, Quaternion.identity);
							urlPos = 0;
						} else if (positions[0].Equals("triumphator")) {
							bier = Instantiate(bierPicta, bierPos, Quaternion.identity);
						} else if (positions[0].Equals("consul")) {
							bier = Instantiate(bierPraetexta, bierPos, Quaternion.identity);
						} else if (positions[0].Equals("censor")) {
							bier = Instantiate(bierPurpurea, bierPos, Quaternion.identity);
						} else if (positions[0].Equals("praetor")) {
							bier = Instantiate(bierPraetexta, bierPos, Quaternion.identity);
						}
						bier.GetComponent<WebLinkFuneral>().url = "" + deceasedID;
						bier.transform.SetParent(thisFuneral.transform);
												
					}
					funeralCount++;
					xPos += 3;
					if (positions.Length - 2 >= endCount) {
						ancestorFuneralCount++;
					}
					for (int i = positions.Length - 2; i >= endCount; i -= 2) {
						//All remaining positions are of ancestors.  Choose the right model and assign the right link
						GameObject toga = null;
						if (positions[i].Equals("triumphator")) {
							toga = Instantiate(togaPicta, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
						} else if (positions[i].Equals("consul")) {
							toga = Instantiate(togaPraetexta, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
						} else if (positions[i].Equals("censor")) {
							toga = Instantiate(togaPurpurea, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
						} else if (positions[i].Equals("praetor")) {
							toga = Instantiate(togaPraetexta, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
						}
						toga.GetComponent<WebLinkFuneral>().url = positions[i+1];
						toga.transform.SetParent(thisFuneral.transform);
						xPos += xDiff;
					}
					if (positions.Length - 2 >= endCount || !collapseAncestorless) {
						zPos += zDiff;
					}
				} 
			}
			xPos = baseX;
			zPos = baseZ - 3;
			GameObject textObjFinal = Instantiate(text3D, new Vector3(xPos, baseValues.y + 2, zPos - 10), Quaternion.identity);
			textObjFinal.GetComponent<TextMesh>().text = "" + (eraEnd - 24) + " to " + eraEnd + "\n" + funeralCount + " funerals\n" + ancestorFuneralCount + " with ancestors";
			textObjFinal.transform.SetParent(parent.transform);
			
			//Uncomment the below line to make a prefab
			Object prefab = PrefabUtility.CreatePrefab(outputFileName, parent);
		}
	}
	
	void Awake () {
		//Uncomment the below line to generate new models
		generateVisualization();
	}
}
