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
/// This class handles object texture swapping.
/// </summary>
public class TextureSwapper : MonoBehaviour {

    #region Fields
    /// <summary>
    /// An array of textures to swap between.
    /// </summary>
    public Texture2D[] textures;
    /// <summary>
    /// The index of the current texture.
    /// </summary>
    int index = 0;
    /// <summary>
    /// The material of this object.
    /// </summary>
    Material material;
    #endregion

    #region Unity Messages
    /// <summary>
    /// This message is called when the script is called.
    /// </summary>
    void Start() {
        material = GetComponent<Renderer>().material;
        SetTexture(index);
    }
    #endregion

    #region Methods
    /// <summary>
    /// A method to set the texture to the previous one in the array.
    /// </summary>
    public void PreviousTexture() {
        index--;
        if(index <= 0) {
            index = textures.Length;
        }
        SetTexture(index);
    }
    /// <summary>
    /// A method to set the texture to the next one in the array.
    /// </summary>
    public void NextTexture() {
        index++;
        if(index >= textures.Length) {
            index = 0;
        }
        SetTexture(index);
    }
    /// <summary>
    /// A method to set the texture to a specified index in the array.
    /// </summary>
    /// <param name="i">
    /// The index of the texture.
    /// </param>
    public void SetTexture(int i) {
        if(i < 0 || i > textures.Length) {
            Debug.LogWarning("Can't set texture: Index "+ i +" does not exist");
            return;
        }
        material.mainTexture = textures[i];
    }
    #endregion

}
