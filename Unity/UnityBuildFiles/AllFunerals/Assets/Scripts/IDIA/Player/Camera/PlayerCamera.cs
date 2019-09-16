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

public class PlayerCamera : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The first person mode.
    /// </summary>
    public FirstPersonMode firstPersonMode;
    /// <summary>
    /// The third person mode.
    /// </summary>
    public ThirdPersonMode thirdPersonMode;
    /// <summary>
    /// The birds eye mode.
    /// </summary>
    public BirdsEyeMode birdsEyeMode;
	/// <summary>
    /// The birds eye mode.
    /// </summary>
    public GodsEyeMode godsEyeMode;
    /// <summary>
    /// The camera mode.
    /// </summary>
    [HideInInspector]
    public IPlayerCameraMode mode;
    /// <summary>
    /// The camera.
    /// </summary>
    [HideInInspector]
    public Camera cam;
	/// <summary>
    /// A flag that a transition was from FirstPersonMode to ThirdPersonMode.
    /// </summary>
    [HideInInspector]
    public bool firstToThird = false;
    /// <summary>
    /// The key to decrease field of view.
    /// </summary>
    public KeyCode fovMinus = KeyCode.Plus;
    /// <summary>
    /// The key to increase field of view.
    /// </summary>
    public KeyCode fovPlus = KeyCode.Minus;
    /// <summary>
    /// The key to look up.
    /// </summary>
	private KeyCode lookUp = KeyCode.Q;
    /// <summary>
    /// The key to look down.
    /// </summary>
	private KeyCode lookDown = KeyCode.Tab;
    /// <summary>
    /// The angle to look up or down.
    /// </summary>
    public float lookStep = 30f;
	/// <summary>
    /// The field of view.
    /// </summary>
    public float fieldOfView = 60f;
    /// <summary>
    /// The current look offset.
    /// </summary>
    float offset = 0f;
    #endregion

    #region Unity Messages
    /// <summary>
	/// A message called when the script instance is being loaded.
	/// </summary>
    void Awake() {
        firstPersonMode.SetPlayerCamera(this);
        thirdPersonMode.SetPlayerCamera(this);
        birdsEyeMode.SetPlayerCamera(this);
		godsEyeMode.SetPlayerCamera(this);
        cam = GetComponent<Camera>();
    }
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        mode = birdsEyeMode;
        cam = GetComponent<Camera>();
    }
    /// <summary>
    /// A message called when the script updates.
    /// </summary>
	void Update() {
		FirstPersonMode.mouseDragDelta.x = (Input.mousePosition.x / Screen.width - 0.5f) * 60.0f;
		FirstPersonMode.mouseDragDelta.y = (Input.mousePosition.y / Screen.height - 0.5f) * -64.0f;
        mode.Update();
		if (EventSystem.current.currentSelectedGameObject) {
			return;
		}
        ChangeFOV();
        ChangeLook();
    }
    /// <summary>
    /// A message called after the script updates.
    /// </summary>
    void LateUpdate() {
		float zoom = birdsEyeMode.zoom;
		if (mode == thirdPersonMode) {
			cam.transform.localEulerAngles = new Vector3(offset, cam.transform.localEulerAngles.y,cam.transform.localEulerAngles.z);
		} else if (mode == birdsEyeMode) {
			//cam.transform.localEulerAngles = new Vector3(offset + 26, cam.transform.localEulerAngles.y,cam.transform.localEulerAngles.z);
			//cam.transform.localEulerAngles = Vector3.Lerp(cam.transform.localEulerAngles, new Vector3(offset + Mathf.Min(-0.000100756f*zoom*zoom*zoom + 0.00588636f*zoom*zoom + 0.0989394f*zoom + 32f, 32f), cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z), Time.deltaTime * birdsEyeMode.cameraTransitionSpeed);
			
			//Alter the camera angle based on the zoom to keep the avatar in focus.
			float angle = 0;
			if (zoom >= 84f) {
				angle = Mathf.Min(-0.000100756f*zoom*zoom*zoom + 0.00588636f*zoom*zoom + 0.0989394f*zoom + 40f, 40f);
			} else {
				angle = Mathf.Atan(13.5f/24f);
			}
			cam.transform.localEulerAngles = new Vector3(offset + Mathf.Min(-0.000100756f*zoom*zoom*zoom + 0.00588636f*zoom*zoom + 0.0989394f*zoom + 40f, 40f), cam.transform.localEulerAngles.y, cam.transform.localEulerAngles.z);
		} else if (mode == godsEyeMode) {
			cam.transform.localEulerAngles = new Vector3(offset + 32, cam.transform.localEulerAngles.y,cam.transform.localEulerAngles.z);
		} else {
			cam.transform.localEulerAngles = new Vector3(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y,cam.transform.localEulerAngles.z);
		}
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to change the field of view.
    /// </summary>
    void ChangeFOV() {
        if(Input.GetKey(fovMinus)) {
            fieldOfView -= .1f;
        }
        if(Input.GetKey(fovPlus)) {
            fieldOfView += .1f;
        }
		if(fieldOfView < 25f) {
			fieldOfView = 25f;
		}
		if(fieldOfView > 100f) {
			fieldOfView = 100f;
		}
		if (mode == firstPersonMode) {
			cam.fieldOfView = 25f + (fieldOfView-25f)*0.8f;
		} else {
			cam.fieldOfView = fieldOfView;
		}
    }
    /// <summary>
    /// A method to change the look angle.
    /// </summary>
    void ChangeLook() {
        if (mode != firstPersonMode) {
			if(Input.GetKeyDown(lookUp)) {
				//Debug.Log("lookup");
				offset = Mathf.Clamp(offset-lookStep, -lookStep, lookStep);
			}
			if(Input.GetKeyDown(lookDown)) {
				//Debug.Log("look down");
				offset = Mathf.Clamp(offset + lookStep, -lookStep, lookStep);
			}
		}
    }
    #endregion

}
