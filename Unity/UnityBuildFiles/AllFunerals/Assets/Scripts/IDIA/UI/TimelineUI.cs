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
/// This class handles the Timeline UI.
/// </summary>
public class TimelineUI : MonoBehaviour {

    #region Fields
    /// <summary>
    /// The temporal slider.
    /// </summary>
    public Slider temporalSlider;
    /// <summary>
    /// The temporal input field.
    /// </summary>
    public InputField temporalInput;
    #endregion

    #region Unity Messages
    /// <summary>
    /// This message is called when this script starts.
    /// </summary>
    void Start() {
        SetTemporalInputText(temporalSlider.value);
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to set the temporal input field text.
    /// </summary>
    /// <param name="time"></param>
    public void SetTemporalInputText(float time) {
        temporalInput.text = ((int)time).ToString();
    }
    /// <summary>
    /// A method to set the temporal slider value.
    /// </summary>
    /// <param name="timeString"></param>
    public void SetTemporalSliderValue(string timeString) {
        float time;
        float.TryParse(timeString, out time);
        time = Mathf.Clamp(time, temporalSlider.minValue, temporalSlider.maxValue);
        temporalSlider.value = time;
    }
    #endregion

}
