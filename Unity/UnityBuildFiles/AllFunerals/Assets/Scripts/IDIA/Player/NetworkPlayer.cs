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
///  This class handles the networking of important player properties.
/// </summary>
public class NetworkPlayer : Photon.MonoBehaviour{

	#region Fields
	/// <summary>
	///  An instance of the PlayerController component on the player.
	/// </summary>
    PlayerController controllerScript;
	/// <summary>
	///  The current scene of the remote player.
	/// </summary>
	int sceneID = 0;
    /// <summary>
    /// Should hide other players?
    /// </summary>
    bool hideOtherPlayers = false;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script is being loaded.
    /// </summary>
    void Awake(){
		DontDestroyOnLoad(gameObject); //Make this player survive scene changes
        controllerScript = GetComponent<PlayerController>(); //Get the PlayerController component
		gameObject.name = gameObject.name + photonView.viewID; //Add the players network id to the end of their gameobject name
    }
    /// <summary>
    /// A message called when this script updates.
    /// </summary>
	void Update(){
        if(!photonView.isMine) {
            ToggleAvatars(!hideOtherPlayers);
        }
		if (!photonView.isMine && !hideOtherPlayers){
            ToggleAvatars(sceneID == Application.loadedLevel);
		}
        if(Input.GetKeyDown(KeyCode.RightControl)) {
            hideOtherPlayers = !hideOtherPlayers;
        }
	}
    #endregion

    #region Photon Messages
    /// <summary>
    /// A message called when Photon serializes the view.
    /// </summary>
    /// <param name="stream">
    /// The Photon stream.
    /// </param>
    /// <param name="info">
    /// The Photon message info.
    /// </param>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
        if (stream.isWriting){ //If this is the local player, send the remote players our animation, position, rotation, and scene
            stream.SendNext(controllerScript.CharState);
			stream.SendNext(MainMenuUI.changedName);
			MainMenuUI.changedName = false;
			stream.SendNext(Application.loadedLevel);

		}else{ //If this is the remote player, receive their animation, position, rotation, and scene
            controllerScript.CharState = (PlayerController.CharacterState)(int)stream.ReceiveNext();
			PlayersUI.refresh = (bool)stream.ReceiveNext();
			sceneID = (int)stream.ReceiveNext();
        }
    }
    /// <summary>
    /// A message called when the local player leaves the room.
    /// </summary>
    void OnLeftRoom() {
        PhotonNetwork.Destroy(photonView.gameObject);
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to toggle avatars on or off.
    /// </summary>
    /// <param name="on">
    /// Toggle avatars on or off?
    /// </param>
    void ToggleAvatars(bool on) {
        Renderer[] rs = transform.GetComponentsInChildren<Renderer>();
        foreach(Renderer r in rs) {
            r.enabled = on; //Turn on/off each renderer on the player depending on their relative scene
        }
        Collider[] cs = transform.GetComponentsInChildren<Collider>();
        foreach(Collider c in cs) {
            c.enabled = on; //Turn on/off each collider on the player depending on their relative scene
        }
    }
    #endregion

}