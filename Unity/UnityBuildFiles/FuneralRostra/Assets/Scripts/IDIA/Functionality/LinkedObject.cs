// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
    
/// <summary>
///  This class constructs a Linked Object Click Event.
/// </summary>
[System.Serializable]
public class LinkedObjectClickEvent : UnityEvent { }

/// <summary>
///  This class handles linked objects.
/// </summary>
public class LinkedObject : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The distance within which the linked object can be affected.
    /// </summary>
    public float distance;
    /// <summary>
    ///  The color to tint the object when highlighted.
    /// </summary>
    public Color hoverColor;
    /// <summary>
    ///  The event to invoke when the linked object is clicked.
    /// </summary>
    public LinkedObjectClickEvent OnLinkClick;
    /// <summary>
    ///  The colors to return to the object when unhighlighted.
    /// </summary>
    List<List<Color>> originalColors;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        originalColors = new List<List<Color>>();
        if (!EventSystem.current.IsPointerOverGameObject()) {
            GetOriginalObjectColors(transform);
        }
    }
    /// <summary>
    ///  This message is called when the collider on the object to which this script is attached is first hovered by the mouse. 
    /// </summary>
    void OnMouseEnter() {
        if (!EventSystem.current.IsPointerOverGameObject() && IsWithinDistance(distance)) {
            SetObjectColors(transform, hoverColor);
        }
    }
    /// <summary>
    ///  This message is called when the collider on the object to which this script is attached is hovered hovered by the mouse. 
    /// </summary>
    void OnMouseOver() {
        if (!EventSystem.current.IsPointerOverGameObject() && IsWithinDistance(distance)) {
            SetObjectColors(transform, hoverColor);
        } else {
            SetObjectColors(transform, originalColors);
        }
    }
    /// <summary>
    ///  This message is called when the collider on the object to which this script is attached is clicked. 
    /// </summary>
    void OnMouseDown() {
        if (!EventSystem.current.IsPointerOverGameObject() && IsWithinDistance(distance)) {
            OnLinkClick.Invoke();
            SetObjectColors(transform, originalColors);
        }
    }
    /// <summary>
    ///  This message is called when the collider on the object to which this script is attached is no longer hovered by the mouse. 
    /// </summary>
    void OnMouseExit() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            SetObjectColors(transform, originalColors);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to highlight the object.
    /// </summary>
    public void Highlight() {
        SetObjectColors(transform, hoverColor);
    }
    /// <summary>
    /// A method to unhighlight the object.
    /// </summary>
    public void UnHighlight() {
        SetObjectColors(transform, originalColors);
    }
    /// <summary>
    /// A method to set the tint colors of the object.
    /// </summary>
    /// <param name="t">
	/// The transform of the target object.
	/// </param>
    /// <param name="c">
	/// The color to tint the target object.
	/// </param>
    void SetObjectColors(Transform t, Color c) {
        MeshRenderer[] renderers = t.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            for (int j = 0; j < renderers[i].materials.Length; j++) {
                renderers[i].materials[j].color = c;
            }
        }
    }
    /// <summary>
    /// A method to set the tint colors of the object.
    /// </summary>
    /// <param name="t">
	/// The transform of the target object.
	/// </param>
    /// <param name="c">
	/// The colors to tint the target object.
	/// </param>
    void SetObjectColors(Transform t, List<List<Color>> c) {
        MeshRenderer[] renderers = t.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            for (int j = 0; j < renderers[i].materials.Length; j++) {
                renderers[i].materials[j].color = c[i][j];
            }
        }
    }
    /// <summary>
    /// A method to get the original tint colors of the object.
    /// </summary>
    /// <param name="t">
	/// The transform of the target object.
	/// </param>
    void GetOriginalObjectColors(Transform t) {
        MeshRenderer[] renderers = t.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++) {
            originalColors.Add(new List<Color>());
            for (int j = 0; j < renderers[i].materials.Length; j++) {
                originalColors[i].Add(renderers[i].materials[j].color);
            }
        }
    }
    /// <summary>
    /// A method to determine whether the camera is within the specified distance of the object.
    /// </summary>
    /// <param name="_distance">
	/// The distance to check.
	/// </param>
    /// <returns>
    /// true or false
    /// </returns>
    bool IsWithinDistance(float _distance) {
        if (!Camera.main) {
            return false;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, _distance);
    }
    #endregion

}
