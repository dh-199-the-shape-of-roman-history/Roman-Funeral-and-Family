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

[System.Serializable]
public class FirstPersonMode : IPlayerCameraMode {

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
    /// <summary>
    /// Is the mouse dragging?
    /// </summary>
    bool isMouseDragging;
    /// <summary>
    /// The (x,y) change in mouse position since the start of the mouse drag.
    /// </summary>
    public static Vector2 mouseDragDelta;
    /// <summary>
    /// The camera look rotation.
    /// </summary>
    Quaternion camLookRotation;
    /// <summary>
    /// The look rotation.
    /// </summary>
    Quaternion lookRotation;
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
        Debug.LogWarning("Can't transition to the same camera mode!");
    }
    /// <summary>
    /// A method to switch the camera mode to third person mode.
    /// </summary>
    public void ToThirdPersonMode() {
		pC.firstToThird = true;
		pC.mode = pC.thirdPersonMode;
    }
    /// <summary>
    /// A method to update the camera mode.
    /// </summary>
    public void Update() {
        CheckMouseWheel();
        CheckArrowKeys();
        Look();
        UpdateCamera();
    }
    /// <summary>
    /// A method to check the mouse wheel.
    /// </summary>
    void CheckMouseWheel() {
        if (Input.mouseScrollDelta.y < 0) {
            ToThirdPersonMode();
        }
    }
    /// <summary>
    /// A mthod to check the arrow keys.
    /// </summary>
    void CheckArrowKeys() {
		if (EventSystem.current.currentSelectedGameObject) {
			return;
		}
        if (Input.GetKeyDown(KeyCode.End)) {
            ToThirdPersonMode();
        }
    }
    /// <summary>
    /// A method to look in the direction of the mouse drag.
    /// </summary>
    void Look() {
        //mouseDragDelta.x += Input.GetAxis("Mouse X");
        //mouseDragDelta.y -= Input.GetAxis("Mouse Y");
    }
    /// <summary>
    /// A method to update the camera.
    /// </summary>
    void UpdateCamera() {
		if (mouseDragDelta.y > 12.0f || mouseDragDelta.y < -12.0f) {
			float ySign = mouseDragDelta.y > 0 ? 1 : -1;
			camLookRotation = Quaternion.Euler(ySign * (Mathf.Abs(mouseDragDelta.y) - 12.0f) * (Mathf.Abs(mouseDragDelta.y) - 12.0f) * (Mathf.Abs(mouseDragDelta.y) - 12.0f) / 160, 0f, 0f);
		}
		else 
			camLookRotation = Quaternion.Euler(0f, 0f, 0f);
		if (mouseDragDelta.x > 10.0f || mouseDragDelta.x < -10.0f) {
			float xSign = mouseDragDelta.x > 0 ? 1 : -1;
			lookRotation = Quaternion.Euler(pC.transform.parent.rotation.eulerAngles.x, pC.transform.parent.rotation.eulerAngles.y + xSign * Mathf.Min(70.0f, (Mathf.Abs(mouseDragDelta.x) - 10.0f) * (Mathf.Abs(mouseDragDelta.x) - 10.0f) * (Mathf.Abs(mouseDragDelta.x) - 10.0f) / 65), pC.transform.parent.rotation.eulerAngles.z);
		}
		else
			lookRotation = pC.transform.parent.rotation;
        camLookRotation = Quaternion.Euler(ClampAngle(camLookRotation.eulerAngles.x, 332f, 28f), 0f, 0f);
		
        //pC.transform.position = Vector3.Lerp(pC.transform.position, transform.position, Time.deltaTime * cameraTransitionSpeed);
		pC.transform.position = transform.position;
		//Debug.Log(pC.transform.localRotation.eulerAngles.x + " " + camLookRotation.eulerAngles.x);
        pC.transform.localRotation = Quaternion.Lerp(pC.transform.localRotation, camLookRotation, Time.deltaTime * cameraTransitionSpeed);
        pC.transform.parent.rotation = Quaternion.Lerp(pC.transform.parent.rotation, lookRotation, Time.deltaTime * cameraTransitionSpeed);
        //pC.cam.cullingMask = mask;
    }
    /// <summary>
    /// A method to clamp an angle.
    /// </summary>
    /// <param name="angle">
    /// The angle to clamp.
    /// </param>
    /// <param name="min">
    /// The minimum value the angle can be.
    /// </param>
    /// <param name="max">
    /// The maximum value the angle can be.
    /// </param>
    /// <returns>
    /// The clamped angle.
    /// </returns>
    float ClampAngle(float angle, float min, float max) {

        if(angle < 90 || angle > 270) { 
            if(angle > 180) angle -= 360; 
            if(max > 180) max -= 360;
            if(min > 180) min -= 360;
        }
        angle = Mathf.Clamp(angle, min, max);
        if(angle < 0) angle += 360;
        return angle;
    }
    #endregion
}
