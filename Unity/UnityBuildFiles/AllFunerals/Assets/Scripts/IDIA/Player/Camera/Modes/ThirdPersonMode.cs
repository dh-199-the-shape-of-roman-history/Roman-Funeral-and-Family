// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class ThirdPersonMode : IPlayerCameraMode {

    #region Fields
    /// <summary>
    /// The player camera.
    /// </summary>
    PlayerCamera pC;
    /// <summary>
    /// The transform of the camera mode.
    /// </summary>
    public Transform transform;
    /// <summary>
    /// The layer mask of the camera mode.
    /// </summary>
    public LayerMask mask;
    /// <summary>
    /// The speed at which the camera transitions to this mode.
    /// </summary>
    public float cameraTransitionSpeed;
    #endregion

    #region Methods
    /// <summary>
    /// A method to set the player camera.
    /// </summary>
    /// <param name="pCam">
    /// The player camera.
    /// </param>
    public void SetPlayerCamera(PlayerCamera pCam) {
        pC = pCam;
    }
    /// <summary>
    /// A method to switch the camera mode to gods eye mode.
    /// </summary>
    public void ToGodsEyeMode() {
        pC.mode = pC.godsEyeMode;
    }
	/// <summary>
    /// A method to switch the camera mode to birds eye mode.
    /// </summary>
    public void ToBirdsEyeMode() {
        pC.mode = pC.birdsEyeMode;
    }
    /// <summary>
    /// A method to switch the camera mode to first person mode.
    /// </summary>
    public void ToFirstPersonMode() {
        pC.mode = pC.firstPersonMode;
    }
    /// <summary>
    /// A method to switch the camera mode to third person mode.
    /// </summary>
    public void ToThirdPersonMode() {
        Debug.LogWarning("Can't transition to the same camera mode!");
    }
    /// <summary>
    /// A method to update the camera mode.
    /// </summary>
    public void Update() {
        CheckMouseWheel();
        CheckArrowKeys();
        UpdateCamera();
    }
    /// <summary>
    /// A method to check the mouse wheel.
    /// </summary>
    void CheckMouseWheel() {
        if (Input.mouseScrollDelta.y < 0) {
            ToBirdsEyeMode();
        } else if(Input.mouseScrollDelta.y > 0) {
            ToFirstPersonMode();
        }
    }
    /// <summary>
    /// A mthod to check the arrow keys.
    /// </summary>
    void CheckArrowKeys() {
		if (EventSystem.current.currentSelectedGameObject) {
			return;
		}
        if (Input.GetKeyDown(KeyCode.Home)) {
            ToFirstPersonMode();
        }
        if (Input.GetKeyDown(KeyCode.End)) {
            ToBirdsEyeMode();
        }
    }
    /// <summary>
    /// A method to update the camera.
    /// </summary>
    void UpdateCamera() {
        if (!pC.firstToThird) {
			pC.transform.position = Vector3.Lerp(pC.transform.position, transform.position, Time.deltaTime * cameraTransitionSpeed);
		} else {
			pC.transform.position = transform.position;
			pC.firstToThird = false;
		}
        pC.transform.rotation = Quaternion.Lerp(pC.transform.rotation, transform.rotation, Time.deltaTime * cameraTransitionSpeed);
        pC.cam.cullingMask = mask;
    }
    #endregion
}