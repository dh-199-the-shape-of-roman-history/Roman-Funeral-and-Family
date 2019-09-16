// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// Programmed by Benjamin Niedzielski (bniedzie@ucla.edu)
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;

/// <summary>
/// This class handles the Compass UI functionality.
/// </summary>
public class Compass : MonoBehaviour {

	/**
	 * Every frame, rotate the camera based on the angle between the camera and north.
	 * This solution does not work close to either pole.
	 */
    public void Update() {
		float rotation = 0.0f;
		if (GameObject.FindWithTag("MainCamera") != null) {
			GameObject camera = GameObject.FindWithTag("MainCamera");
			float sign = (camera.transform.forward.x < Vector3.forward.x)? -1.0f : 1.0f;
			rotation = Vector2.Angle(new Vector2(camera.transform.forward.x, camera.transform.forward.z), new Vector2(0.0f, 1.0f)) * sign;
		}
		transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
	}

}
