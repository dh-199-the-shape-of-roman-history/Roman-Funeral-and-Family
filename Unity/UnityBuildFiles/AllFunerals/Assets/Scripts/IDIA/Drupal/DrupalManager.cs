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
///  This class manages the data that comes from the Drupal Unity Interface.
/// </summary>
public class DrupalManager : MonoBehaviour {
    
    #region Class Fields
    /// <summary>
    ///  An instance of the Drupal Unity Interface.
    /// </summary>
    DrupalUnityIO drupalUnityIO;
    /// <summary>
    ///  The current environment in the Drupal Unity Interface.
    /// </summary>
    public Environment currentEnvironment;
    /// <summary>
    ///  The current tour in the Drupal Unity Interface.
    /// </summary>
    public Tour currentTour;
    #endregion

    #region Unity Messages
    /// <summary>
    ///  A message called when this script is enabled.
    /// </summary>
    void OnEnable() {
        DrupalUnityIO.OnGotCurrentEnvironment += OnGotCurrentEnvironment;
    }
    /// <summary>
    ///  A message called when this script is being loaded.
    /// </summary>
    void Awake() {
        drupalUnityIO = FindObjectOfType<DrupalUnityIO>();
    }
    /// <summary>
    ///  A message called when this script starts.
    /// </summary>
    void Start () {
        drupalUnityIO.GetCurrentEnvironment();
	}
    /// <summary>
    ///  A message called when this script is disabled.
    /// </summary>
    void OnDisable() {
        DrupalUnityIO.OnGotCurrentEnvironment -= OnGotCurrentEnvironment;
    }
    #endregion

    #region Callbacks
    /// <summary>
    ///  A callback called when the Drupal Unity Interface received the current environment.
    /// </summary>
    /// <param name="environment">
	/// The received environment.
	/// </param>
    void OnGotCurrentEnvironment(Environment environment) {
        currentEnvironment = environment;
        currentTour = environment.tours[0];
    }
    #endregion

}
