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
/// This class handles objects that should face the camera.
/// </summary>
public class FaceCamera : MonoBehaviour {

    #region Unity Messages
    /// <summary>
    /// A message called when this script updates.
    /// </summary>
    void Update () {
        //transform.LookAt(Camera.main.transform);
		transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
	}
    #endregion

}
