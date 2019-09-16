// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using DrupalUnity;

/// <summary>
///  This class handles city engine objects.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CityEngineBuilding : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The city engine object ID.
    /// </summary>
    int objID;
    #endregion

    #region Unity Messages
    /// <summary>
    ///  This message is called when the script starts. 
    /// </summary>
    void Start() {
        if(!int.TryParse(gameObject.name.Substring(1, 5), out objID)) {
            Debug.LogError("Wasn't able to parse City Engine object name!");
        }
    }
    /// <summary>
    ///  This message is called when the collider on the object to which this script is attached is clicked. 
    /// </summary>
    void OnMouseDown() {
        Application.ExternalCall("window.open", "http://mandala.shanti.virginia.edu/places/"+objID+"/overview/nojs", "_blank");
    }
    #endregion

}
