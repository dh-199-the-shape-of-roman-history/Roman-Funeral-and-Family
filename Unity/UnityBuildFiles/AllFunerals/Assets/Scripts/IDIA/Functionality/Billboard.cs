using UnityEngine;
using System.Collections;

/**
 * This script causes any object with it attached to turn to face the main camera.  This is useful for 2-dimensional text.
 * Author: Benjamin Niedzielski (bniedzie@ucla.edu)
 */
public class Billboard : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Camera.main != null) {
			Vector3 targetPostition = new Vector3( Camera.main.transform.position.x, 
                                        this.transform.position.y, 
                                        Camera.main.transform.position.z ) ;
			this.transform.LookAt( targetPostition ) ;
		}
		Vector3 angle = transform.eulerAngles;
		transform.eulerAngles = new Vector3(angle.x, angle.y + 180, angle.z);
	}
}
