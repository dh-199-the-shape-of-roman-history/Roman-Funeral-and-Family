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
/// This class allows a web page to be opened when the player enters a trigger.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ProximityTrigger : MonoBehaviour {

	#region Fields
	/// <summary>
	/// The URL that should be opened.
	/// </summary>
	public string url;
	#endregion

	#region Unity Messages
	/// <summary>
	/// A message called when the script instance is being loaded.
	/// </summary>
	void Awake(){
		gameObject.layer = 2; //We must put this object on the IgnoreRaycast layer, so we do not block clicks
	}

    /// <summary>
    /// A message called when a collider has entered a trigger on this object.
    /// </summary>
    /// <param name="c">
    /// The other collider.
    /// </param>
    void OnTriggerEnter(Collider c){
		if(c.tag=="LocalPlayer"){ //If the it was the local player's collider
			//if(Application.isWebPlayer){ //If this is the web player
				Application.ExternalEval("window.open('"+url+"','_blank')"); //Open the URL in a new tab
			/*}else{ //If this is not the web player
				Application.OpenURL(url); //Open the URL in the browser
			}*/
		}
	}
	#endregion

}
