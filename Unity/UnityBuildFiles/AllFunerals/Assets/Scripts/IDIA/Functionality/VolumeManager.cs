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
using System.Collections.Generic;

/// <summary>
/// This class handles volume control.
/// </summary>
public class VolumeManager : MonoBehaviour {

    #region Fields
    /// <summary>
    /// A dictionary of audio sources in the scene.
    /// </summary>
    Dictionary<AudioSource, float> audios = new Dictionary<AudioSource, float>();
    #endregion

    #region Unity Messages
    /// <summary>
    /// This message is called when the script starts.
    /// </summary>
    void Start() {
        LoadAudioSources();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to load the audio sources in the scene.
    /// </summary>
    void LoadAudioSources() {
        foreach(AudioSource a in FindObjectsOfType<AudioSource>()) {
            audios.Add(a, a.volume);
        }
    }
    /// <summary>
    /// A method to set the volume by a percentage.
    /// </summary>
    /// <param name="percentage">
    /// The percentage to set the volume.
    /// </param>
    public void SetVolume(float percentage) {
        foreach(KeyValuePair<AudioSource, float> a in audios) {
            a.Key.volume = a.Value * percentage;
        }
    }
    #endregion

}
