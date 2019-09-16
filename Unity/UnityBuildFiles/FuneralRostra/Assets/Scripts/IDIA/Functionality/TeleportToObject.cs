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
/// This class allows for teleportation by clicking on a collider.
/// </summary>
public class TeleportToObject : MonoBehaviour {

	#region Fields
	/// <summary>
	/// The transform of the local player.
	/// </summary>
	Transform player;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        player = GameObject.FindGameObjectWithTag("LocalPlayer").transform; //Get the local player's transform
    }
    /// <summary>
    /// A message called when the object is clicked.
    /// </summary>
    void OnMouseDown(){
		player.position = transform.position;
	}
	#endregion

	#region Photon Messages
	/// <summary>
	/// A message called when the local player leaves a room.
	/// </summary>
	void OnLeftRoom(){
		player = null;
	}
	#endregion

}
