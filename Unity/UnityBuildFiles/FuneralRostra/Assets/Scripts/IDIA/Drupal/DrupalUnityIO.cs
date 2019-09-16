// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using System.Text.RegularExpressions;
using LitJson;

/// <summary>
///  This namespace contains Drupal Unity Interface classes.
/// </summary>
namespace DrupalUnity {
    /// <summary>
    ///  This class constructs an Environment object.
    /// </summary>
    [System.Serializable]
    public class Environment {
        /// <summary>
        ///  The id of this environment.
        /// </summary>
        public int id;
        /// <summary>
        ///  The title of this environment.
        /// </summary>
        public string title;
        /// <summary>
        ///  The description of this environment.
        /// </summary>
        public string description;
        /// <summary>
        ///  The starting location in this environment.
        /// </summary>
        public Location starting_location;
        /// <summary>
        ///  The array of tours in this environment.
        /// </summary>
        public Tour[] tours;
    }
    /// <summary>
    ///  This class constructs a Tour object.
    /// </summary>
    [System.Serializable]
    public class Tour {
        /// <summary>
        ///  The id of this tour.
        /// </summary>
        public int id;
        /// <summary>
        ///  The title of this tour.
        /// </summary>
        public string title;
        /// <summary>
        ///  The description of this tour.
        /// </summary>
        public string description;
        /// <summary>
        ///  The array of placards in this tour.
        /// </summary>
        public Placard[] placards;
        /// <summary>
        ///  The unity binary of this tour.
        /// </summary>
        public string unity_binary;
    }
    /// <summary>
    ///  This class constructs a Placard object.
    /// </summary>
    [System.Serializable]
    public class Placard {
        /// <summary>
        ///  The id of this placard.
        /// </summary>
        public int id;
        /// <summary>
        ///  The title of this placard.
        /// </summary>
        public string title;
        /// <summary>
        ///  The description of this placard.
        /// </summary>
        public string description;
        /// <summary>
        ///  The location of this placard.
        /// </summary>
        public Location location;
        /// <summary>
        ///  The layer of this placard.
        /// </summary>
        public string layer;
        /// <summary>
        ///  The image url of this placard.
        /// </summary>
        public string image_url;
    }
    /// <summary>
    ///  This class constructs a Location object.
    /// </summary>
    [System.Serializable]
    public class Location {
        /// <summary>
        ///  The latitude of this location.
        /// </summary>
        public double latitude;
        /// <summary>
        ///  The longitude of this location.
        /// </summary>
        public double longitude;
        /// <summary>
        ///  The elevation of this location.
        /// </summary>
        public double elevation;
        /// <summary>
        ///  The orientation of this location.
        /// </summary>
        public double orientation;
    }
    /// <summary>
    ///  This class constructs a Status object.
    /// </summary>
    [System.Serializable]
    public class Status {
        /// <summary>
        ///  The success of this status.
        /// </summary>
        public bool success;
    }

    /// <summary>
    ///  This class manages input to and output from the Drupal Unity Interface
    /// </summary>
    public class DrupalUnityIO : MonoBehaviour {

        #region Events
        /// <summary>
        ///  The delegate to handle an added placard.
        /// </summary>
        /// <param name="added">
        /// The received Status.
        /// </param>
        public delegate void AddedP(Status added);
        /// <summary>
        ///  The event to invoke when a placard is added.
        /// </summary>
        public static event AddedP OnAddedPlacard;
        /// <summary>
        ///  The delegate to handle getting the current environment.
        /// </summary>
        /// <param name="currentEnvironment">
        /// The received current environment.
        /// </param>
        public delegate void GotCE(Environment currentEnvironment);
        /// <summary>
        ///  The event to invoke when the current environment has been received.
        /// </summary>
        public static event GotCE OnGotCurrentEnvironment;
        /// <summary>
        ///  The delegate to handle getting the current placard ID.
        /// </summary>
        /// <param name="placardId">
        /// The received placard ID.
        /// </param>
        public delegate void GotCPId(int placardId);
        /// <summary>
        ///  The event to invoke when the current placard ID has been received.
        /// </summary>
        public static event GotCPId OnGotCurrentPlacardId;
        /// <summary>
        ///  The delegate to handle getting the current tour ID.
        /// </summary>
        /// <param name="currentTourId">
        /// The received current tour ID.
        /// </param>
        public delegate void GotCTId(int currentTourId);
        /// <summary>
        ///  The event to invoke when the current tour ID has been received.
        /// </summary>
        public static event GotCTId OnGotCurrentTourId;
        /// <summary>
        ///  The delegate to handle getting an environment.
        /// </summary>
        /// <param name="tour">
        /// The received environment.
        /// </param>
        public delegate void GotE(Environment environment);
        /// <summary>
        ///  The event to invoke when an environment has been received.
        /// </summary>
        public static event GotE OnGotEnvironment;
        /// <summary>
        ///  The delegate to handle getting placards.
        /// </summary>
        /// <param name="placards">
        /// The received placards.
        /// </param>
        public delegate void GotPs(Placard[] placards);
        /// <summary>
        ///  The event to invoke when placards have been received.
        /// </summary>
        public static event GotPs OnGotPlacards;
        /// <summary>
        ///  The delegate to handle getting a tour.
        /// </summary>
        /// <param name="tour">
        /// The received tour.
        /// </param>
        public delegate void GotT(Tour tour);
        /// <summary>
        ///  The event to invoke when a tour has been received.
        /// </summary>
        public static event GotT OnGotTour;
        /// <summary>
        ///  The delegate to handle selecting a placard.
        /// </summary>
        /// <param name="placard">
        /// The selected placard.
        /// </param>
        public delegate void PlacardS(Placard placard);
        /// <summary>
        ///  The event to invoke when a placard has been selected.
        /// </summary>
        public static event PlacardS OnPlacardSelected;
        #endregion

        #region Fields
        /// <summary>
        ///  The regex for matching HTML tags.
        /// </summary>
        string tagRegex = "(<\\/?(?:\\s|\\S)*?>)";
#if UNITY_EDITOR
        bool debug = true;
#else
        bool debug=false;
#endif
        public TextAsset addPlacardText, currentEnvironmentText, currentPlacardIdText, currentTourIdText, environmentText, placardsText, tourText;
        #endregion

        #region Unity Messages
        /// <summary>
        /// A message called when the script instance starts.
        /// </summary>
        void Start() {
            AddEventListener(gameObject.name, "PlacardSelected", "placard_selected");
        }
        #endregion

        #region Methods
        /// <summary>
        /// A method to add a placard.
        /// </summary>
        /// <param name="placard">
        /// The placard to add.
        /// </param>
        public void AddPlacard(Placard placard) {
            if (debug) {
                AddedPlacard(addPlacardText.text);
                return;
            }
            string placard_json = JsonMapper.ToJson(placard);
            Application.ExternalCall("DrupalUnityInterface.addPlacard", gameObject.name, "AddedPlacard", placard_json);
        }
        /// <summary>
        /// A method to get the current environment.
        /// </summary>
        public void GetCurrentEnvironment() {
            if (debug) {
                GotCurrentEnvironment(currentEnvironmentText.text);
                return;
            }
            Application.ExternalCall("DrupalUnityInterface.getCurrentEnvironment", gameObject.name, "GotCurrentEnvironment");
        }
        /// <summary>
        /// A method to get the current placard ID.
        /// </summary>
        public void GetCurrentPlacardId() {
            if (debug) {
                Debug.LogWarning("GotCurrentPlacardId doesn't have a debug text yet!");
                return;
            }
            Application.ExternalCall("DrupalUnityInterface.getCurrentPlacardId", gameObject.name, "GotCurrentPlacardId");
        }
        /// <summary>
        /// A method to get the current tour ID.
        /// </summary>
        public void GetCurrentTourId() {
            if (debug) {
                Debug.LogWarning("GotCurrentTourId doesn't have a debug text yet!");
                return;
            }
            Application.ExternalCall("DrupalUnityInterface.getCurrentTourId", gameObject.name, "GotCurrentTourId");
        }
        /// <summary>
        /// A method to get an environment.
        /// </summary>
        /// <param name="environment_id">
        /// The ID of the environment to get.
        /// </param>
        public void GetEnvironment(int environment_id) {
            if (debug) {
                Debug.LogWarning("GotEnvironment doesn't have a debug text yet!");
                return;
            }
            string environment_id_json = JsonMapper.ToJson(environment_id);
            Application.ExternalCall("DrupalUnityInterface.getEnvironment", gameObject.name, "GotEnvironment", environment_id_json);
        }
        /// <summary>
        /// A method to get placards.
        /// </summary>
        /// <param name="placard_ids">
        /// The IDs of the placards to get.
        /// </param>
        public void GetPlacards(int[] placard_ids) {
            if (debug) {
                Debug.LogWarning("GotPlacards doesn't have a debug text yet!");
                return;
            }
            string placard_id_json = JsonMapper.ToJson(placard_ids);
            Application.ExternalCall("DrupalUnityInterface.getPlacards", gameObject.name, "GotPlacards", placard_id_json);
        }
        /// <summary>
        /// A method to get a tour.
        /// </summary>
        /// <param name="tour">
        /// The ID of the tour to get.
        /// </param>
        public void GetTour(int tour_id) {
            if (debug) {
                GotTour(tourText.text);
                return;
            }
            string tour_id_json = JsonMapper.ToJson(tour_id);
            Application.ExternalCall("DrupalUnityInterface.getTour", gameObject.name, "GotTour", tour_id_json);
        }
        /// <summary>
        /// A method to select a placard.
        /// </summary>
        /// <param name="placard">
        /// The placard to select.
        /// </param>
        public void SelectPlacard(Placard placard) {
            if (debug) {
				Debug.Log("Hello");
                OnPlacardSelected(placard);
                return;
            }
            string placard_json = JsonMapper.ToJson(placard);
            TriggerEvent("placard_selected",placard_json);
        }
        /// <summary>
        /// A method to add an event listener.
        /// </summary>
        /// <param name="gameObjectName">
        /// The gameObject on which to call the callback.
        /// </param>
        /// /// <param name="callback">
        /// The callback to call.
        /// </param>
        /// /// <param name="eventName">
        /// The name of the event on which to listen.
        /// </param>
        public void AddEventListener(string gameObjectName, string callback, string eventName) {
            if (debug) {
                return;
            }
            Application.ExternalCall("DrupalUnityInterface.addEventListener", gameObjectName, callback, eventName);
        }
        /// <summary>
        /// A method to trigger an event.
        /// </summary>
        /// <param name="eventName">
        /// The name of the event to trigger.
        /// </param>
        /// /// <param name="jsonArgs">
        /// The JSON-formatted arguments to send.
        /// </param>
        public void TriggerEvent(string eventName, string jsonArgs) {
            if (debug) {
                return;
            }
            Application.ExternalCall("DrupalUnityInterface.triggerEvent", eventName, jsonArgs);
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// A callback called when a placard is added.
        /// </summary>
        /// <param name="json">
        /// The status as json.
        /// </param>
        public void AddedPlacard(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            Status added;
            try {
                added = JsonMapper.ToObject<Status>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnAddedPlacard != null) {
                OnAddedPlacard(added);
            }
        }
        /// <summary>
        /// A callback called when the current enviroment has been received.
        /// </summary>
        /// <param name="json">
        /// The environment as json.
        /// </param>
        public void GotCurrentEnvironment(string json) {
            json = Regex.Replace(json, tagRegex, "");
            Environment currentEnvironment;
            try {
                currentEnvironment = JsonMapper.ToObject<Environment>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if(OnGotCurrentEnvironment != null) {
                OnGotCurrentEnvironment(currentEnvironment);
            }
        }
        /// <summary>
        /// A callback called when the current placard ID has been received.
        /// </summary>
        /// <param name="json">
        /// The current placard ID as json.
        /// </param>
        public void GotCurrentPlacardId(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            int currentPlacardId;
            try {
                currentPlacardId = JsonMapper.ToObject<int>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnGotCurrentPlacardId != null) {
                OnGotCurrentPlacardId(currentPlacardId);
            }
        }
        /// <summary>
        /// A callback called when the current tour ID has been received.
        /// </summary>
        /// <param name="json">
        /// The current tour ID as json.
        /// </param>
        public void GotCurrentTourId(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            int currentTourId;
            try {
                currentTourId = JsonMapper.ToObject<int>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnGotCurrentTourId != null) {
                OnGotCurrentPlacardId(currentTourId);
            }
        }
        /// <summary>
        /// A callback called when an environment has been received.
        /// </summary>
        /// <param name="json">
        /// The environment as json.
        /// </param>
        public void GotEnvironment(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            Environment environment;
            try {
                environment = JsonMapper.ToObject<Environment>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnGotEnvironment != null) {
                OnGotEnvironment(environment);
            }
        }
        /// <summary>
        /// A callback called when placards have been received.
        /// </summary>
        /// <param name="json">
        /// The array of placards as json.
        /// </param>
        public void GotPlacards(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            Placard[] placards;
            try {
                placards = JsonMapper.ToObject<Placard[]>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnGotPlacards != null) {
                OnGotPlacards(placards);
            }
        }
        /// <summary>
        /// A callback called when a tour has been received.
        /// </summary>
        /// <param name="json">
        /// The tour as json.
        /// </param>
        public void GotTour(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            Tour tour;
            try {
                tour = JsonMapper.ToObject<Tour>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnGotTour != null) {
                OnGotTour(tour);
            }
        }
        /// <summary>
        /// A callback called when a placard is selected.
        /// </summary>
        /// <param name="json">
        /// The placards as json.
        /// </param>
        public void PlacardSelected(string json) {
            json = Regex.Replace(json, tagRegex, ""); 
            Placard placard;
            try {
                placard = JsonMapper.ToObject<Placard>(json);
            } catch(JsonException e) {
                Debug.LogError(e.ToString());
                Application.ExternalCall("console.exception", "[UNITY WEBGL] " + e.ToString());
                return;
            }
            if (OnPlacardSelected != null) {
				Debug.Log("Hello");
                OnPlacardSelected(placard);
            }
        }
        #endregion
    }
}
