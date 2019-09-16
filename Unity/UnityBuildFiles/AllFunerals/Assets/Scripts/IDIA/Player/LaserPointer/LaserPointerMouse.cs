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
/// This class handles the laser pointer mouse functionality.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserPointerMouse : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The transform at which to begin the laser pointer.
    /// </summary>
    public Transform root;
    /// <summary>
    /// The line renderer.
    /// </summary>
    LineRenderer lr;
    /// <summary>
    /// Is the laser pointer enabled?
    /// </summary>
    bool laserPointerEnabled = false;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        lr = GetComponent<LineRenderer>();
        lr.SetVertexCount(2);
        lr.SetPosition(0, root.position);
    }
    /// <summary>
    /// A message called when the script updates.
    /// </summary>
    void Update() {
        if (Input.GetKeyDown(KeyCode.L) && !EventSystem.current.currentSelectedGameObject) {
            laserPointerEnabled = !laserPointerEnabled;
        }
        if (laserPointerEnabled) {
            lr.SetPosition(0, root.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Camera.main.farClipPlane)) {
                lr.SetPosition(1, hit.point);
            } else {
                lr.SetPosition(1, ray.direction* Camera.main.farClipPlane);
            }
        } else {
            lr.SetPosition(0, root.position);
            lr.SetPosition(1, root.position);
        }
    }
    #endregion

}
