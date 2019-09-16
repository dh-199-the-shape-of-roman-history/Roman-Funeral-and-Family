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
using UnityEngine.Events;
using DrupalUnity;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
///  This class constructs a Placard Event.
/// </summary>
[System.Serializable]
public class PlacardEvent : UnityEvent<Placard> { }

/// <summary>
///  This class manages incoming placards.
/// </summary>
public class PlacardManager : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The placard prefab.
    /// </summary>
    public GameObject placardPrefab;
	/// <summary>
    ///  Placard IDs that cause the player to not teleport when selected.
    /// </summary>
    public int[] nonMovingPlacardIDs;
    /// <summary>
    ///  The array of incoming placards.
    /// </summary>
    Placard[] placards;
    /// <summary>
    ///  The array of placard objects.
    /// </summary>
    List<GameObject> placardObjects = new List<GameObject>();
    /// <summary>
    ///  The event to invoke when a placard is selected.
    /// </summary>
    public PlacardEvent OnPlacardSelected;
    /// <summary>
    ///  An instance of the Drupal Unity Interface.
    /// </summary>
    DrupalUnityIO drupalUnityIO;
    /// <summary>
    ///  An instance of a canvas to hold the placard UI prefabs.
    /// </summary>
    Canvas canvas;
    #endregion

    #region Unity Messages
    /// <summary>
	/// A message called when the script instance is enabled.
	/// </summary>
    void OnEnable() {
        DrupalUnityIO.OnGotCurrentEnvironment += OnGotCurrentEnvironment;
        DrupalUnityIO.OnGotTour += OnGotTour;
        DrupalUnityIO.OnPlacardSelected += OnPlacardWasSelected;
    }
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        drupalUnityIO = FindObjectOfType<DrupalUnityIO>();
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
    }
    /// <summary>
    /// A message called when the script updates.
    /// </summary>
    void Update() {
        if (!canvas.worldCamera) {
            canvas.worldCamera = Camera.main;
        }
    }
    /// <summary>
	/// A message called when the script instance is disabled.
	/// </summary>
    void OnDisable() {
        DrupalUnityIO.OnGotCurrentEnvironment -= OnGotCurrentEnvironment;
        DrupalUnityIO.OnGotTour -= OnGotTour;
        DrupalUnityIO.OnPlacardSelected -= OnPlacardWasSelected;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// A callback called when the Drupal Unity Interface gets an environment.
    /// </summary>
    /// <param name="environment">
    /// The received environment.
    /// </param>
    void OnGotCurrentEnvironment(Environment environment) {
        ClearPlacards();
        placards = environment.tours[0].placards;
        GeneratePlacards();
    }
    /// <summary>
    /// A callback called when the Drupal Unity Interface gets a tour.
    /// </summary>
    /// <param name="tour">
	/// The received tour.
	/// </param>
    void OnGotTour(Tour tour) {
        ClearPlacards();
        placards = tour.placards;
        GeneratePlacards();
    }
    /// <summary>
    /// A callback called when the Drupal Unity Interface selects a placard.
    /// </summary>
    /// <param name="placard">
	/// The selected placard.
	/// </param>
    void OnPlacardWasSelected(Placard placard) {
        if(OnPlacardSelected != null) {
            OnPlacardSelected.Invoke(placard);
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to generate placard objects for each of the placards received.
    /// </summary>
    void GeneratePlacards() {
        int count = 0;
        foreach(Placard p in placards) {
            count++;
            Placard placard = p; //capture the iterator; otherwise you always get last placard
			if (!nonMovingPlacardIDs.Contains(placard.id)) {
				GameObject newPlacard = (GameObject)Instantiate(placardPrefab, Vector3.zero, Quaternion.identity);
				newPlacard.transform.position = GeographicManager.Instance.GetPosition(placard.location.latitude, placard.location.longitude, placard.location.elevation);
				newPlacard.transform.rotation.eulerAngles.Set(0f, (float)placard.location.orientation, 0f);
				newPlacard.GetComponent<RectTransform>().SetParent(transform, true);
				newPlacard.GetComponent<PlacardObject>().placard = placard;
				newPlacard.GetComponent<Text>().text = "#"+count;
				placardObjects.Add(newPlacard);
				EventTrigger trigger = newPlacard.GetComponent<EventTrigger>();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerClick;
				entry.callback.AddListener((e) => { drupalUnityIO.SelectPlacard(placard); });
				trigger.triggers.Add(entry);
			}
        }
    }
    /// <summary>
    /// A method to clear the placards array and destroy placard objects.
    /// </summary>
    void ClearPlacards() {
        placards = null;
        foreach(GameObject pO in placardObjects) {
            Destroy(pO);
        }
        placardObjects.Clear();
    }
    #endregion

}
