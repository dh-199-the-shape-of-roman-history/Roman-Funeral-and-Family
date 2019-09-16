// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;

/// <summary>
/// This class allows for hyperlinking clicks on structures to the correct placard.
/// </summary>
public class WebLinkTree : MonoBehaviour {

	#region Fields
	/// <summary>
	/// The URL.
	/// </summary>
	#endregion

	#region Unity Messages
	/// <summary>
	/// A message called when the object is clicked.
	/// </summary>
	public void click(){
		string site = "sites/default/files/trees/";
		GameObject models = null;
		if (GameObject.FindWithTag("Conservative") != null) {
			models = GameObject.FindWithTag("Conservative");
			site += "conservative/";
		} else if (GameObject.FindWithTag("Aggressive") != null) {
			models = GameObject.FindWithTag("Aggressive");
			site += "aggressive/";
		} else {
			models = GameObject.FindWithTag("Hyperaggressive");
			site += "hyperaggressive/";
		} 
		site += models.GetComponent<FuneralControl>().treeURLs[GameObject.FindWithTag("FuneralGen").GetComponent<FuneralGenerator>().currentFuneral];
		Application.ExternalEval("window.open('" + site + "', '_blank')"); //Just open the URL in the browser
	}
	#endregion
	
}
