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
/// 
/// </summary>
public class WebManager : MonoBehaviour {

    #region Methods
    /// <summary>
    /// A method to open the url in the default browser.
    /// </summary>
    /// <param name="url">
    /// The URL to open.
    /// </param>
    public void OpenURL(string url) {
        Application.OpenURL(url);
    }

#if UNITY_WEBGL
    /// <summary>
    /// A method to open a new tab in the current browser.
    /// </summary>
    /// <param name="url">
    /// The URL to open.
    /// </param>
    public void OpenURLInNewTab(string url) {
        Application.ExternalCall("window.open",url,"_blank");
    }
#endif
    #endregion

}
