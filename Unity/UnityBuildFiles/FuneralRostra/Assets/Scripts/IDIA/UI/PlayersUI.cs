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
/// This class handles the player list UI.
/// </summary>
public class PlayersUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The player container gameObject.
    /// </summary>
    public GameObject players;
    /// <summary>
    /// The player prefab.
    /// </summary>
    public GameObject playerPrefab;
    /// <summary>
    /// The selected player.
    /// </summary>
    [HideInInspector]
    public PhotonPlayer selectedPlayer;
	[HideInInspector]
	public static bool refresh = false;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start () {
        RefreshPlayers();
	}
	
	void Update () {
        if (refresh) {
			RefreshPlayers();
			refresh = false;
		}
	}
    #endregion

    #region Photon Messages
    /// <summary>
    /// A message called when a player connects to the Photon room.
    /// </summary>
    /// <param name="newPlayer">
    /// The player.
    /// </param>
    void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        RefreshPlayers();
    }
    /// <summary>
    /// A message called when a player disconnects from the Photon room.
    /// </summary>
    /// <param name="otherPlayer">
    /// The player.
    /// </param>
    void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer) {
        RefreshPlayers();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to refresh the player list.
    /// </summary>
    public void RefreshPlayers() {
        for(int i = 0; i < players.transform.childCount; i++) {
            Toggle playerToggle = players.transform.GetChild(i).GetComponent<Toggle>();
            playerToggle.onValueChanged.RemoveAllListeners();
            Destroy(players.transform.GetChild(i).gameObject);
        }
        foreach(PhotonPlayer player in PhotonNetwork.playerList) {
            GameObject playerObj = Instantiate(playerPrefab);
            playerObj.transform.SetParent(players.transform);
            playerObj.transform.Find("Name").GetComponent<Text>().text = player.name;
            Toggle playerToggle = playerObj.GetComponent<Toggle>();
			Navigation newNav = new Navigation();
			newNav.mode = Navigation.Mode.None;
			playerToggle.navigation = newNav;
            playerToggle.group = players.GetComponent<ToggleGroup>();
            playerToggle.onValueChanged.AddListener((on) => {
				PhotonPlayer selected = null;
				foreach(PhotonPlayer player2 in PhotonNetwork.playerList) {
					if (player2.name.Equals(playerObj.transform.Find("Name").GetComponent<Text>().text)) {
						selected = player2;
						break;
					}
				}
				selectedPlayer = on ? selected : null;
			});
        }
    }
    #endregion

}
