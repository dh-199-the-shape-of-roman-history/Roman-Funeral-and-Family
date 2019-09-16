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
/// This class handles the map UI.
/// </summary>
public class MapUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The local player.
    /// </summary>
    GameObject player;
    /// <summary>
    /// The scene changer.
    /// </summary>
    SceneChanger sceneChanger;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script updates.
    /// </summary>
    void Update () {
        player = GameObject.FindGameObjectWithTag("LocalPlayer");
        if (!sceneChanger) {
            sceneChanger = FindObjectOfType<SceneChanger>();
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to change the scene.
    /// </summary>
    /// <param name="sceneName">
    /// The scene name.
    /// </param>
    public void ChangeScene(string sceneName) {
        sceneChanger.LoadScene(sceneName);
    }
    /// <summary>
    /// A method to telelport the local player.
    /// </summary>
    /// <param name="t">
    /// The local player transform.
    /// </param>
    public void TeleportPlayer(Transform t) {
        player.transform.position = t.position;
        player.transform.rotation = t.rotation;
    }
    #endregion

}
