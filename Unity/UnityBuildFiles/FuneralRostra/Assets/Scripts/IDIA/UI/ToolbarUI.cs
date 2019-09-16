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
/// This class handles the Toolbar UI functionality.
/// </summary>
public class ToolbarUI : MonoBehaviour {

    #region Methods
    /// <summary>
    /// A method to open Join.Me.
    /// </summary>
    public void JoinMe() {
        Application.OpenURL("joinme://");
    }
    /// <summary>
    /// A method to exit the room.
    /// </summary>
    public void Exit() {
        AvatarPlacardController.isMoving = false;
		PhotonNetwork.LeaveRoom();
    }
    #endregion

}
