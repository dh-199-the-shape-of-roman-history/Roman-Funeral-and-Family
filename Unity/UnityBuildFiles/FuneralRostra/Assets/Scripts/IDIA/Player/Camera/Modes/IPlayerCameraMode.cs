// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------

/// <summary>
/// This interface defines camera modes.
/// </summary>
public interface IPlayerCameraMode {

    #region Methods
    /// <summary>
    /// A method to update the camera mode.
    /// </summary>
    void Update();
    /// <summary>
    /// A method to switch the camera mode to first person mode.
    /// </summary>
    void ToFirstPersonMode();
    /// <summary>
    /// A method to switch the camera mode to third person mode.
    /// </summary>
    void ToThirdPersonMode();
    /// <summary>
    /// A method to switch the camera mode to birds eye mode.
    /// </summary>
    void ToBirdsEyeMode();
	/// <summary>
    /// A method to switch the camera mode to gods eye mode.
    /// </summary>
    void ToGodsEyeMode();
    /// <summary>
    /// A method to set the player camera.
    /// </summary>
    /// <param name="pCam">
    /// The player camera.
    /// </param>
    void SetPlayerCamera(PlayerCamera pCam);
    #endregion
}
