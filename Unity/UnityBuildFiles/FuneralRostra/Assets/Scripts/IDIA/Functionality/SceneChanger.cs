// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

/// <summary>
///  This singleton class handles scene changes.
/// </summary>
public class SceneChanger : Photon.MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The instance of this singleton class.
    /// </summary>
    public static SceneChanger Instance {
        get;
        private set;
    }
    /// <summary>
    ///  The loading screen UI gameObject.
    /// </summary>
    public GameObject loadingScreen;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when the script instance is being loaded.
    /// </summary>
    void Awake() {
        //DontDestroyOnLoad(loadingScreen);
        if(Instance == null) { //If there is no instance of this script, then set the instance to be this script and make the gameobject survive scene changes
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else { //If there is already an instance of this script, then destroy this gameobject
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// A message called when the script instance is updated.
    /// </summary>
    void Update() {
        if(!loadingScreen) {
            loadingScreen = GameObject.Find("LoadingScreen");
        }
    }
    #endregion

    #region Photon Messages
    /// <summary>
    /// A message called when the local player leaves the room.
    /// </summary>
    void OnLeftRoom() {
        LoadScene(0);
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to load a scene.
    /// </summary>
    /// <param name="sceneName">
    /// The name of the scene to load.
    /// </param>
    public void LoadScene(string sceneName) {
        StartCoroutine(DoLoadScene(sceneName));
    }
    /// <summary>
    /// A method to load a scene.
    /// </summary>
    /// <param name="sceneID">
    /// The ID of the scene to load.
    /// </param>
    public void LoadScene(int sceneID) {
        StartCoroutine(DoLoadScene(sceneID));
    }
    /// <summary>
    /// A coroutine to load a scene asynchronously.
    /// </summary>
    /// <param name="sceneName">
    /// The name of the scene to load.
    /// </param>
    IEnumerator DoLoadScene(string sceneName) {
#if UNITY_EDITOR || UNITY_STANDALONE
        loadingScreen.SetActive(true);
        AsyncOperation async = Application.LoadLevelAsync(sceneName);
        while(!async.isDone) {
            yield return new WaitForSeconds(.5f);
        }
        loadingScreen.SetActive(false);
        yield return async;
#elif UNITY_WEBPLAYER || UNITY_WEBGL
        loadingScreen.SetActive(true);
        while (!Application.CanStreamedLevelBeLoaded(sceneName)) {
            yield return new WaitForSeconds(.1f);
        }
        AsyncOperation async = Application.LoadLevelAsync(sceneName);
        while(!async.isDone) {
            yield return new WaitForSeconds(.1f);
        }
        loadingScreen.SetActive(false);
        yield return async;
#endif
    }
    /// <summary>
    /// A coroutine to load a scene asynchronously.
    /// </summary>
    /// <param name="sceneID">
    /// The ID of the scene to load.
    /// </param>
    IEnumerator DoLoadScene(int sceneID) {
#if UNITY_EDITOR || UNITY_STANDALONE
        loadingScreen.SetActive(true);
        AsyncOperation async = Application.LoadLevelAsync(sceneID);
        while(!async.isDone) {
            yield return new WaitForSeconds(.5f);
        }
        loadingScreen.SetActive(false);
        yield return async;
#elif UNITY_WEBPLAYER || UNITY_WEBGL
        loadingScreen.SetActive(true);
        while (!Application.CanStreamedLevelBeLoaded(sceneID)) {
            yield return new WaitForSeconds(.1f);
        }
        AsyncOperation async = Application.LoadLevelAsync(sceneID);
        while(!async.isDone) {
            yield return new WaitForSeconds(.1f);
        }
        loadingScreen.SetActive(false);
        yield return async;
#endif
    }
    #endregion

}
