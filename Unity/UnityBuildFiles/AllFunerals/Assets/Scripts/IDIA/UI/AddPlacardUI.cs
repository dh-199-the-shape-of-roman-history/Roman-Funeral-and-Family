// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using DrupalUnity;

/// <summary>
/// This class handles the Add Placard UI window.
/// </summary>
public class AddPlacardUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The title input field.
    /// </summary>
    public InputField titleInput;
    /// <summary>
    /// The description input field.
    /// </summary>
    public InputField descriptionInput;
    /// <summary>
    /// The latitude input field.
    /// </summary>
    public InputField latitudeInput;
    /// <summary>
    /// The longitude input field.
    /// </summary>
    public InputField longitudeInput;
    /// <summary>
    /// The elevation input field.
    /// </summary>
    public InputField elevationInput;
    /// <summary>
    /// The orientation input field.
    /// </summary>
    public InputField orientationInput;
    /// <summary>
    /// The submit button.
    /// </summary>
    public Button submitButton;
    /// <summary>
    /// The Drupal Unity Interface.
    /// </summary>
    DrupalUnityIO drupalIO;
    /// <summary>
    /// The local player transform.
    /// </summary>
    Transform player;
    /// <summary>
    /// The latitude of the player.
    /// </summary>
    double latitude;
    /// <summary>
    /// The longitude of the player.
    /// </summary>
    double longitude;
    #endregion


    #region Unity Messages
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start() {
        drupalIO = FindObjectOfType<DrupalUnityIO>();
    }
    /// <summary>
    /// A message called when this script updates.
    /// </summary>
	void Update () {
        submitButton.interactable = titleInput.text.Length > 0;
        if (!player) {
            player = GameObject.FindGameObjectWithTag("LocalPlayer").transform;
        } else {
            GetLatLong(player);
            latitudeInput.text = (((int)(latitude * 1000000.0))/1000000.0).ToString();
            longitudeInput.text = (((int)(longitude * 1000000.0))/1000000.0).ToString();
            elevationInput.text = (((int)(player.transform.position.y * 1000000.0))/1000000.0).ToString();
            orientationInput.text = ((int)(player.rotation.eulerAngles.y)).ToString();
        }
	}
    #endregion

    #region Methods
    /// <summary>
    /// A method to get the latitude and longitude of the local player.
    /// </summary>
    /// <param name="t">
    /// The local player transform.
    /// </param>
    void GetLatLong(Transform t) {
		//Known: origin 0,0,0
		//known: current z, x.
		//Process: convert origin lat/long to mercator
		//Missing step: scalefactor.
		//Add offset, convert back to lat/long
		//Use geomarker somehow.
		Vector3 local_pos = GeographicManager.Instance.GetCurrentMarkerMercator(t.position.z, t.position.x);
        GeographicCoord.MercatorEllipticalToDecimalDegrees(out latitude, out longitude, local_pos.z, local_pos.x);
		//GeographicCoord.MercatorSphericalToDecimalDegrees(out latitude, out longitude, t.position.z, t.position.x);
    }
    /// <summary>
    /// A method to create a placard.
    /// </summary>
    public void CreatePlacard() {
        Placard newPlacard = new Placard();
        newPlacard.location = new Location();
        newPlacard.title = titleInput.text;
        newPlacard.description = descriptionInput.text;
        newPlacard.location.latitude = float.Parse(latitudeInput.text);
        newPlacard.location.longitude = float.Parse(longitudeInput.text);
        newPlacard.location.elevation = float.Parse(elevationInput.text);
		newPlacard.location.orientation = (float.Parse(orientationInput.text));
        drupalIO.AddPlacard(newPlacard);
    }
    /// <summary>
    /// A method to clear the input fields.
    /// </summary>
    public void ClearFields() {
        titleInput.text = "";
        descriptionInput.text = "";
    }
	
	/// <summary>
	/// Toggles the window's visibility.
	/// </summary>
	public void ToggleVisibility() {
		gameObject.SetActive(!gameObject.activeSelf);
	}
    #endregion

}
