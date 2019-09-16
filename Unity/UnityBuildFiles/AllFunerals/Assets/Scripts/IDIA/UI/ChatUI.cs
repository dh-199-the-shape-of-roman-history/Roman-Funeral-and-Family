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
/// This class handles the chat UI.
/// </summary>
public class ChatUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The messages container gameObject.
    /// </summary>
    public GameObject messages;
    /// <summary>
    /// The message prefab.
    /// </summary>
    public GameObject messagePrefab;
    /// <summary>
    /// The chat input field.
    /// </summary>
    public InputField chatInput;
    /// <summary>
    /// The instance of the Chat manager.
    /// </summary>
    Chat chat;
    /// <summary>
    /// The players UI.
    /// </summary>
    PlayersUI playersUI;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script is enabled.
    /// </summary>
    void OnEnable() {
        Chat.OnGotChat += OnGotChat;
    }
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start() {
        chat = FindObjectOfType<Chat>();
        playersUI = FindObjectOfType<PlayersUI>();
    }
	/// <summary>
    /// A message called when this script updates.
    /// </summary>
    void Update() {
		if (chatInput.IsActive() && Input.GetKeyDown(KeyCode.Return)) {
			SendChat();
		}
    }
    /// <summary>
    /// A message called when this script is disabled.
    /// </summary>
    void OnDisable() {
        Chat.OnGotChat -= OnGotChat;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// A callback called when a chat message has been received.
    /// </summary>
    /// <param name="message">
    /// The received message.
    /// </param>
    private void OnGotChat(string message) {
        RefreshChat();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to send a chat message.
    /// </summary>
    public void SendChat() {
        if (chatInput.text.Length > 0) {
            if (playersUI.selectedPlayer!=null) {
                chat.SendChat(playersUI.selectedPlayer, PhotonNetwork.player, chatInput.text);
            } else {
                chat.SendChat(PhotonTargets.All, chatInput.text);
            }
        }
        ClearInput();
		chatInput.Select();
		chatInput.ActivateInputField();
    }
    /// <summary>
    /// A method to clear the chat input.
    /// </summary>
    void ClearInput() {
        chatInput.text = "";
    }
    /// <summary>
    /// A method to refresh the chat messages.
    /// </summary>
    public void RefreshChat() {
        for (int i = 0; i < messages.transform.childCount; i++) {
            Destroy(messages.transform.GetChild(i).gameObject);
        }
        foreach (string message in Chat.Messages) {
            GameObject messageObj = Instantiate(messagePrefab);
            messageObj.transform.SetParent(messages.transform);
            messageObj.GetComponent<Text>().text = message;
        }
    }
    #endregion

}
