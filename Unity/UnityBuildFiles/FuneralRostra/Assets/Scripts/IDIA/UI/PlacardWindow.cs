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
/// This class handles the placard window.
/// </summary>
public class PlacardWindow : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The placard window gameObject.
    /// </summary>
    public GameObject placardWindow;
    /// <summary>
    /// The placard title.
    /// </summary>
    public Text placardTitleText;
    /// <summary>
    /// The placard description.
    /// </summary>
    public Text placardDescriptionText;
    /// <summary>
    /// The placard teleport button.
    /// </summary>
    public Button placardTeleportButton;
    #endregion

    #region Methods
    /// <summary>
    /// A method to open a placard info window.
    /// </summary>
    /// <param name="placard">
    /// The placard.
    /// </param>
    public void OpenPlacardInfoWindow(Placard placard) {
        placardWindow.SetActive(true);
        placardTitleText.text = placard.title;
        placardDescriptionText.text = placard.description;
        placardTeleportButton.interactable = placard.location != null;
        placardTeleportButton.onClick.AddListener(() => TeleportPlayerToPlacardLocation(placard));
    }

    /// <summary>
    /// A method to teleport the local player to the placard location.
    /// </summary>
    /// <param name="placard">
    /// The placard.
    /// </param>
    public void TeleportPlayerToPlacardLocation(Placard placard) {
        //if (!GetComponent<PlacardManager>().nonMovingPlacardIDs.Contains(placard.id)) {
			Transform localPlayer = GameObject.FindGameObjectWithTag("LocalPlayer").transform;
			localPlayer.position = GeographicManager.Instance.GetPosition(placard.location.latitude, placard.location.longitude, placard.location.elevation);
			localPlayer.rotation = Quaternion.Euler(0f, (float)placard.location.orientation, 0f);
		//}
    }
    #endregion

}
