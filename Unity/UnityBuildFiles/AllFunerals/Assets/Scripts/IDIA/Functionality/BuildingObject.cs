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
using UnityEngine.Events;

/// <summary>
///  This class handles tour and placard dependent objects.
/// </summary>
public class BuildingObject : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  Valid tour IDs.
    /// </summary>
    public int[] validTourIDs;
    /// <summary>
    ///  Valid placard IDs.
    /// </summary>
    public int[] validPlacardIDs;
    /// <summary>
    ///  The event to invoke when an valid tour is received.
    /// </summary>
    public UnityEvent OnValidTour;
    /// <summary>
    ///  The event to invoke when an invalid tour is received.
    /// </summary>
    public UnityEvent OnInvalidTour;
    /// <summary>
    ///  The event to invoke when an valid placard is received.
    /// </summary>
    public UnityEvent OnValidPlacard;
    /// <summary>
    ///  The event to invoke when an invalid placard is received.
    /// </summary>
    public UnityEvent OnInvalidPlacard;
    #endregion

    #region Unity Messages
    /// <summary>
    ///  This message is called when the script is enabled. 
    /// </summary>
    void OnEnable() {
        DrupalUnityIO.OnPlacardSelected += OnPlacardSelected;
        DrupalUnityIO.OnGotTour += OnGotTour;
    }
    /// <summary>
    ///  This message is called when the script is disabled. 
    /// </summary>
    void OnDisable() {
        DrupalUnityIO.OnPlacardSelected -= OnPlacardSelected;
        DrupalUnityIO.OnGotTour -= OnGotTour;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// A callback called when the Drupal Unity Interface gets a tour.
    /// </summary>
    /// <param name="tour">
	/// The received tour.
	/// </param>
    void OnGotTour(Tour tour) {
        if(IsValidTourID(tour.id)) {
            OnValidTour.Invoke();
        } else {
            OnInvalidTour.Invoke();
        }
    }
    /// <summary>
    /// A callback called when the Drupal Unity Interface selects a placard.
    /// </summary>
    /// <param name="placard">
	/// The selected placard.
	/// </param>
    void OnPlacardSelected(Placard placard) {
        if (IsValidPlacardID(placard.id)) {
            OnValidPlacard.Invoke();
        } else {
            OnInvalidPlacard.Invoke();
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to determine whether a given placard ID is valid or not.
    /// </summary>
    /// <param name="id">
	/// The placard id.
	/// </param>
    /// <returns>
    /// true or false
    /// </returns>
    bool IsValidPlacardID(int id) {
        return validPlacardIDs.Contains(id);
    }
    /// <summary>
    /// A method to determine whether a given placard ID is valid or not.
    /// </summary>
    /// <param name="id">
	/// The tour id.
	/// </param>
    /// <returns>
    /// true or false
    /// </returns>
    bool IsValidTourID(int id) {
        return validTourIDs.Contains(id);
    }
    #endregion

}
