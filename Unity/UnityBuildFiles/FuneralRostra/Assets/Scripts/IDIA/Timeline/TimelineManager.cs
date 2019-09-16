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
/// This class manages the timeline.
/// </summary>
public class TimelineManager : MonoBehaviour {

    #region Events
    /// <summary>
    ///  The delegate to handle a time change.
    /// </summary>
    /// <param name="time">
    /// The new time.
    /// </param>
    public delegate void ChangedTime(float time);
    /// <summary>
    ///  The event to invoke when the time has changed.
    /// </summary>
    public static event ChangedTime OnChangedTime;
    #endregion

    #region Fields
    /// <summary>
    /// The default time.
    /// </summary>
    public float defaultTime; 
    /// <summary>
    /// The time.
    /// </summary>
    float time;
    #endregion

    #region Unity Messages
    /// <summary>
    /// A message called when this script starts.
    /// </summary>
    void Start() {
        time = defaultTime;
        SetTimeline(time);
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to set the timeline.
    /// </summary>
    /// <param name="newTime">
    /// The new time.
    /// </param>
    public void SetTimeline(float newTime) {
        time = newTime;
        if (OnChangedTime!=null) {
            OnChangedTime(newTime);
        }
    }
    #endregion

}
