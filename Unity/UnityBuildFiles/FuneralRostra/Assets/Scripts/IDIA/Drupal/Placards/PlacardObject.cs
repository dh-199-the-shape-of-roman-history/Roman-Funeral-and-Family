// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using DrupalUnity;

/// <summary>
///  This class manages individual placard behavior.
/// </summary>
public class PlacardObject : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The placard data.
    /// </summary>
    public Placard placard;
    /// <summary>
    ///  The local player.
    /// </summary>
    GameObject player;
    /// <summary>
    ///  An instance of the Drupal Unity Interface.
    /// </summary>
    DrupalUnityIO drupalUnityIO;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        player = GameObject.FindGameObjectWithTag("LocalPlayer");
        drupalUnityIO = FindObjectOfType<DrupalUnityIO>();
    }
    /// <summary>
    /// A message called when the script updates.
    /// </summary>
    void Update() {
        if (!player) {
            player = GameObject.FindGameObjectWithTag("LocalPlayer");
        }
    }
    /// <summary>
    /// A message called when a collider enters this object's trigger.
    /// </summary>
    /// /// <param name="col">
	/// The other collider.
	/// </param>
    void OnTriggerEnter(Collider col) {
        if(col.tag == "LocalPlayer") {
            drupalUnityIO.SelectPlacard(placard);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to teleport the local player to the placard's position.
    /// </summary>
    public void TeleportPlayer() {
        player.transform.position = transform.position;
        player.transform.rotation = transform.rotation;
    }
    #endregion



}
