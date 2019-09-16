using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * This script highlights the game object it is attached to if it is moused over,
 * restoring the original colors when the mouse is moved away.
 * Author: Benjamin Niedzielski (bniedzie@ucla.edu)
 */
public class Highlighter : MonoBehaviour {
	
	List<Color> colors;

	// Use this for initialization
	void Start () {
		colors = new List<Color>();
	}
	
	void OnMouseOver()
	{
		bool alreadyHover = true;
		if (colors.Count == 0) {
			alreadyHover = false;
		}
		
		//highlight every texture yellow and save the original textures in a List if needed.
		Renderer renderer = GetComponent<Renderer>();
		foreach (Material material in renderer.materials) {
			if (!alreadyHover)
				colors.Add(material.color);
			material.color = Color.yellow;
		}
		foreach (Transform child in transform)
		{
			foreach (Material material in child.GetComponent<Renderer>().materials) {
				if (!alreadyHover)
					colors.Add(material.color);
				material.color = Color.yellow;
			}
		}
	}

	void OnMouseExit()
	{
		//restore the original colors from the list.  The materials will be travered in the same order each time.
		Renderer renderer = GetComponent<Renderer>();
		foreach (Material material in renderer.materials) {
			material.color = colors[0];
			//material.color = Color.white;
			colors.RemoveAt(0);
		}
		foreach (Transform child in transform)
		{
			foreach (Material material in child.GetComponent<Renderer>().materials) {
				material.color = colors[0];
				//material.color = Color.white;
				colors.RemoveAt(0);
			}
		}
		colors = new List<Color>();
	} 
}
