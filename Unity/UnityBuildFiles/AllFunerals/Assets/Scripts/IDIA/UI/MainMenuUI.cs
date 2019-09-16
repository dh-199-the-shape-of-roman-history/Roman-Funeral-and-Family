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
using System;

/// <summary>
/// This class handles the main menu UI.
/// </summary>
public class MainMenuUI : MonoBehaviour{

    #region Fields
    /// <summary>
    /// The start button.
    /// </summary>
    public Button startButton;
    /// <summary>
    /// The create room button.
    /// </summary>
    public Button createRoomButton;
    /// <summary>
    /// The avatar and name screen gameObject.
    /// </summary>
    public GameObject avatarAndName;
    /// <summary>
    /// The room selection screen gameObject.
    /// </summary>
    public GameObject roomSelection;
    /// <summary>
    /// The rooms container gameObject.
    /// </summary>
    public GameObject rooms;
    /// <summary>
    /// The room prefab.
    /// </summary>
    public GameObject roomPrefab;
    /// <summary>
    /// The connecting window gameObject.
    /// </summary>
    public GameObject connectingWindow;
    /// <summary>
    /// The first scene to load.
    /// </summary>
    public string firstScene;
    /// <summary>
    /// The selected avatar.
    /// </summary>
	public string selectedAvatar = "Adam";

	//For autoroom join. At present the developer must decide between letting users pick a room and having the room be joined automatically. 
	private string roomNameText = "Funeral_Room";
    #endregion
	
	public static bool changedName = false;

    #region Unity Messages
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start() {
        InvokeRepeating("RefreshRooms",0f,5f);
        selectedAvatar = selectedAvatar=="" ? "Adam" : selectedAvatar;
        if(!PhotonNetwork.connected) {
            PhotonNetwork.ConnectUsingSettings("1.0");
            connectingWindow.SetActive(true);
        }
    }
	
	void Update() {
		startButton.interactable = PhotonNetwork.connected;
	}
    #endregion

    #region Photon Messages
    /// <summary>
    /// A message called when the local player connects to the Photon server.
    /// </summary>
    void OnConnectedToPhoton() {
        connectingWindow.SetActive(false);
    }
    /// <summary>
    /// A message called when the local player joins a Photon room.
    /// </summary>
    void OnJoinedRoom() {
        Close();
		
		//Avoid duplicate names by adding a number in parens to distinguish between players
		//This is necessary to ensure that the locks on who can move the stage work properly.
		bool dupName = false;
		foreach (PhotonPlayer player in PhotonNetwork.otherPlayers) {
			if (player.name == PhotonNetwork.playerName) {
				dupName = true;
			}
		}
		if (dupName) {
			int i = 1;
			while (true) {
				dupName = false;
				foreach (PhotonPlayer player in PhotonNetwork.otherPlayers) {
					if (player.name == (PhotonNetwork.playerName + "(" + i + ")")) {
						dupName = true;
					}
				}
				if (!dupName) break;
				i++;
			}
			PhotonNetwork.playerName += "(" + i + ")";
			changedName = true;
		}
		
        SpawnPlayer(selectedAvatar, Vector3.zero, gameObject.transform.rotation, 0); //Spawn the selected avatar at the spawn position
        SceneChanger.Instance.LoadScene(firstScene);
    }
    /// <summary>
    /// A message called when the local player leaves a Photon room.
    /// </summary>
    void OnLeftRoom() {
        Open();
    }
    /// <summary>
    /// A message called when a room list update is received.
    /// </summary>
    void OnReceivedRoomListUpdate() {
        RefreshRooms();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to save the player settings.
    /// </summary>
    void SaveSettings(){
		PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
		PlayerPrefs.SetString("avatar",selectedAvatar);
	}
    /// <summary>
    /// A method to select an avatar.
    /// </summary>
    /// <param name="avatar">
    /// The avatar.
    /// </param>
    public void SelectAvatar(string avatar) {
        selectedAvatar = avatar;
    }
    /// <summary>
    /// A method to set the player name.
    /// </summary>
    /// <param name="name">
    /// The name.
    /// </param>
    public void SetPlayerName(string name) {
        PhotonNetwork.playerName = name;
    }
    /// <summary>
    /// A method to validate the start button.
    /// </summary>
    /// <param name="input">
    /// The input to use for validation.
    /// </param>
    public void ValidateStartButton(string input) {
        //do nothing
    }
    /// <summary>
    /// A method to validate the create room button.
    /// </summary>
    /// <param name="input">
    /// The input to use for validation.
    /// </param>
    public void ValidateCreateRoomButton(string input) {
        createRoomButton.interactable = input.Length > 0;
    }
    /// <summary>
    /// A method to start the game.
    /// </summary>
    public void StartGame() {
        avatarAndName.SetActive(false);
        roomSelection.SetActive(true);
    }

	public void StartGameAuto() {
		avatarAndName.SetActive(false);
		roomSelection.SetActive(false);
	}
    /// <summary>
    /// A method to refresh the room list.
    /// </summary>
    public void RefreshRooms() {
        for(int i = 0; i < rooms.transform.childCount;i++ ) {
            rooms.transform.GetChild(i).transform.Find("JoinButton").GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(rooms.transform.GetChild(i).gameObject);
        }
        foreach(RoomInfo room in PhotonNetwork.GetRoomList()) {
            GameObject roomObj = Instantiate(roomPrefab);
            roomObj.transform.SetParent(rooms.transform);
            roomObj.transform.Find("RoomName").GetComponent<Text>().text = room.name;
            roomObj.transform.Find("RoomPopulation").GetComponent<Text>().text = room.playerCount+"/"+room.maxPlayers;
            roomObj.transform.Find("JoinButton").GetComponent<Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(room.name));
        }
    }
    /// <summary>
    /// A method to create a room.
    /// </summary>
    /// <param name="input">
    /// The name of the room.
    /// </param>
    public void CreateRoom(InputField input) {
        
		roomNameText = input.name;

		if(input.text == "") {
			roomNameText = "Funeral_Room"; 
            //return;
        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.isVisible = true;
        roomOptions.isOpen = true;
        roomOptions.maxPlayers = (byte)25;
        TypedLobby typedLobby = new TypedLobby();
        PhotonNetwork.CreateRoom(input.text,roomOptions,typedLobby);
    }

	/// <summary>
	/// A method to create/join room automatically.
	/// </summary>
	/// <param name="input">
	/// The name of the room.
	/// </param>
	public void CreateJoinRoomAuto() {

		roomNameText = "Funeral_Room";
		int roomID = 0;
		bool exists = true;
		string targetRoom;
		
		//Since each room has a cap of 10 people, find the first room for this project
		//that is not already full
		while (exists) {
			exists = false;
			targetRoom = roomNameText;
			if (roomID != 0) {
				targetRoom += "_" + roomID;
			}
		
			foreach(RoomInfo room in PhotonNetwork.GetRoomList()) {
				Debug.Log(targetRoom + " " + room.name);
				if (targetRoom == room.name) {
					if (room.maxPlayers > room.playerCount) {
						PhotonNetwork.JoinRoom(room.name);
						return; 
					} else {
						exists = true;
						break;
					}
				} 
			}
			Debug.Log("HI!");
			
			if (exists) {
				roomID++;
			}
		}

		targetRoom = roomNameText;
		if (roomID != 0) {
			targetRoom += "_" + roomID;
		}
		
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.isVisible = true;
		roomOptions.isOpen = true;
		roomOptions.maxPlayers = (byte)25;
		TypedLobby typedLobby = new TypedLobby();
		Debug.Log(targetRoom + " 1");
		PhotonNetwork.CreateRoom(targetRoom,roomOptions,typedLobby);
	}
    /// <summary>
    /// A method to spawn the local player.
    /// </summary>
    /// <param name="avatarName">
    /// The avatar prefab name.
    /// </param>
    /// <param name="spawnPosition">
    /// The spawn position.
    /// </param>
    /// <param name="spawnRotation">
    /// The spawn rotation.
    /// </param>
    /// <param name="group">
    /// The group.
    /// </param>
    void SpawnPlayer(string avatarName, Vector3 spawnPosition, Quaternion spawnRotation, int group) {
        GameObject player = PhotonNetwork.Instantiate(avatarName, spawnPosition, spawnRotation, (byte)group);
        player.GetComponent<PlayerController>().enabled = true;
        player.tag = "LocalPlayer";
        player.transform.Find("Camera").gameObject.SetActive(true);
        player.transform.Find("Canvas").gameObject.SetActive(false);
        player.transform.Find("LaserPointer").gameObject.SetActive(true);
        player.GetComponent<AvatarPlacardController>().enabled = true;
        player.GetComponent<DoubleClickTeleport>().enabled = true;
    }
    /// <summary>
    /// A method to open the menu.
    /// </summary>
    public void Open() {
        for(int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// A method to close the menu.
    /// </summary>
    public void Close() {
        for(int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    #endregion

}
