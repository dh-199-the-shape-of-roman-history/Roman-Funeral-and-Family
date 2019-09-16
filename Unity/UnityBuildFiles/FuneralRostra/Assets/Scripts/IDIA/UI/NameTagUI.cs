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

/// <summary>
/// This class handles the name tag UI.
/// </summary>
public class NameTagUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The photon view.
    /// </summary>
    public PhotonView photonView;
    /// <summary>
    /// The name tag.
    /// </summary>
    public Text nameTagText;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script is being loaded.
    /// </summary>
    void Awake () {
        nameTagText.enabled = !photonView.isMine;
        nameTagText.text = photonView.owner.name;
	}
    #endregion

}
