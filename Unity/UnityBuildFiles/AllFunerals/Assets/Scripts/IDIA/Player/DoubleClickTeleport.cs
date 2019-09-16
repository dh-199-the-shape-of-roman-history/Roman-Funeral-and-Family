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
/// This class allows for teleportation by double-clicking on a collider.
/// </summary>
public class DoubleClickTeleport : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The layers that can be double-clicked to teleport.
    /// </summary>
    public LayerMask layers;

    /// <summary>
    /// The radius of the teleportation range.
    /// </summary>
    public float radius = 100f;

	/// <summary>
	/// The hit info of the raycast.
	/// </summary>
	RaycastHit hit;

	/// <summary>
	/// Whether the mouse hovering and in range of a collider.
	/// </summary>
	bool mouseHit;
	#endregion

	#region Unity Messages
	/// <summary>
	/// A message called every fixed framerate frame.
	/// </summary>
	void FixedUpdate(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Construct a ray based upon the mouse position
		mouseHit = Physics.Raycast(ray,out hit,radius,layers.value); //Check that ray for any hits within the range
	}

	/// <summary>
	/// A message called for rendering and handling GUI events.
	/// </summary>
	void OnGUI(){
		Event e = Event.current;
		if(mouseHit && e.isMouse && e.clickCount == 2){ //If the mouse is hovering over something within range and the player double-clicks
			transform.position = hit.point; //Teleport the player to that point
		}
	}
	#endregion
 
}
