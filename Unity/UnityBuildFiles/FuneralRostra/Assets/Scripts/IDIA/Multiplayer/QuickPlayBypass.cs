// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;

/// <summary>
///  This class allows for the enabling of quickplay in the web player.
/// </summary>
public class QuickPlayBypass : MonoBehaviour {

	#region Fields
	/// <summary>
	///  A boolean to keep quickplay from preventing players from leaving a room and going to the lobby.
	/// </summary>
	bool didFirstJoin;
	#endregion

	#region Photon Messages
	/// <summary>
	///  A message called when the local player enters the lobby.
	/// </summary>
	void OnJoinedLobby(){
		if(/*Application.isWebPlayer &&*/ !didFirstJoin){
			GetURL();
			didFirstJoin = true;
		}
	}
    #endregion

    #region Methods
    /// <summary>
    ///  A method to get the url of the page on which the web player is hosted.
    /// </summary>
    void GetURL() {
        Application.ExternalEval("u.getUnity().SendMessage(\"" + name + "\", \"ReceiveURL\", document.URL);");
    }

    /// <summary>
    ///  A method to receive the url from the page on which the web player is hosted.
    /// </summary>
    /// /// <param name="url">
    /// The URL of the page on which the web player is hosted.
    /// </param>
    void ReceiveURL(string url) {
        CheckURL(url);
    }

    /// <summary>
    ///  A method to check the url for a quickplay GET variable.
    /// </summary>
    /// <param name="url">
    /// The URL of the page on which the web player is hosted.
    /// </param>
    void CheckURL(string url) {
        string quickPlayString = url.Substring(url.LastIndexOf("?") + 1);
        if(url.Contains("?quickplay") || quickPlayString.Contains("&quickplay")) {
            SetUpQuickPlay();
        }
    }

    /// <summary>
    ///  A method to set up quickplay in the web player.
    /// </summary>
    void SetUpQuickPlay() {
        PhotonNetwork.JoinRandomRoom();
        PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
    }
    #endregion

}
