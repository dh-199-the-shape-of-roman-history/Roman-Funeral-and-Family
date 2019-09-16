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
/// This class handles the spawn point.
/// </summary>
public class SpawnPoint : MonoBehaviour {

    #region Unity Messages
    /// <summary>
    /// This message is called when a level has been loaded.
    /// </summary>
    /// <param name="level">
    /// The level that was loaded.
    /// </param>
    void OnLevelWasLoaded(int level) {
        SpawnPlayer();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to spawn the local player gameObject.
    /// </summary>
    public void SpawnPlayer() {
        GameObject localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer");
        localPlayer.transform.position = transform.position;
        localPlayer.transform.rotation = transform.rotation;
    }
    #endregion

}
