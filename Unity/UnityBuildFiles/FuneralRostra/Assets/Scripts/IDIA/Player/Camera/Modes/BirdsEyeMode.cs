// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class handles the birds eye mode of the player camera.
/// </summary>
[System.Serializable]
public class BirdsEyeMode : IPlayerCameraMode {

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
	public float zoom = 80f;
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
        Debug.LogWarning("Can't transition to the same camera mode!");
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
        pC.mode = pC.thirdPersonMode;
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
            //ToGodsEyeMode();
			if (zoom < 30f) {
				zoom -= 2f;
			}
			else if (zoom < 50f) {
				zoom -= 1.5f;
			}
			else {
				zoom -= 1f;
			}
			if (zoom < 0f) {
				zoom = 0f;
			}
        } else if(Input.mouseScrollDelta.y > 0) {
            //ToThirdPersonMode();
			if (zoom < 30f)
				zoom += 2f;
			else if (zoom < 50f)
				zoom += 1.5f;
			else {
				zoom += 1f;
			}
			if (zoom > 100f) {
				zoom = 100f;
			}
        }
    }
    /// <summary>
    /// A mthod to check the arrow keys.
    /// </summary>
    void CheckArrowKeys() {
		if (EventSystem.current.currentSelectedGameObject) {
			return;
		}
        /*if (Input.GetKeyDown(KeyCode.Home)) {
            ToThirdPersonMode();
        }
		if (Input.GetKeyDown(KeyCode.End)) {
            ToGodsEyeMode();
        }*/
    }
    /// <summary>
    /// A method to update the camera.
    /// </summary>
    void UpdateCamera() {
		//Vector3 zoomAxis = new Vector3(0.5f, 24f, -13.5f);
		float zoomY = 24f;
		float zoomZ = -13.5f;
        //pC.transform.position = Vector3.Lerp(pC.transform.position, transform.position + zoomAxis * ((80f-zoom)/80f), Time.deltaTime * cameraTransitionSpeed);
		pC.transform.position = Vector3.Lerp(pC.transform.position, transform.position + (new Vector3(pC.transform.forward.x*zoomZ, zoomY, pC.transform.forward.z * zoomZ) * ((80f-zoom)/80f)), Time.deltaTime * cameraTransitionSpeed);
        pC.transform.rotation = Quaternion.Lerp(pC.transform.rotation, transform.rotation, Time.deltaTime * cameraTransitionSpeed);
        pC.cam.cullingMask = mask;
    }
    #endregion

}
