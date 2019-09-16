// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class handles a timeline object.
/// </summary>
public class TimelineObject : MonoBehaviour {

    #region Events
    /// <summary>
    /// The event to trigger if the time has entered a range.
    /// </summary>
    public UnityEvent OnEnteredTime;
    /// <summary>
    /// The event to trigger if the time has exited a range.
    /// </summary>
    public UnityEvent OnExitedTime;
    #endregion

    #region Fields
    /// <summary>
    /// The ranges to watch.
    /// </summary>
    public TimelineRange[] ranges;
    /// <summary>
    /// The current time.
    /// </summary>
    float time;
    /// <summary>
    /// Are we currently within a range?
    /// </summary>
    bool inTime;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script is being loaded.
    /// </summary>
    void Awake() {
        TimelineManager.OnChangedTime += OnChangedTime;
    }
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start() {
        if (InRange(time)) {
            OnEnteredTime.Invoke();
            inTime = true;
        }
        if (InRange(time)) {
            OnExitedTime.Invoke();
            inTime = false;
        }
    }
    /// <summary>
    /// A message called when this script updates.
    /// </summary>
    void Update() {
        if(InRange(time) && !inTime) {
            OnEnteredTime.Invoke();
            inTime = true;
        }
        if (!InRange(time) && inTime) {
            OnExitedTime.Invoke();
            inTime = false;
        }
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// A callback called when the time is changed.
    /// </summary>
    /// <param name="t">
    /// The new time.
    /// </param>
    private void OnChangedTime(float t) {
        time = t;
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to determine if we are within a range.
    /// </summary>
    /// <param name="t">
    /// The time to check.
    /// </param>
    /// <returns>
    /// true or false.
    /// </returns>
    bool InRange(float t) {
        foreach (TimelineRange range in ranges) {
            if (time >= range.minTime && time <= range.maxTime) {
                return true;
            }
        }
        return false;
    }
    #endregion

}
