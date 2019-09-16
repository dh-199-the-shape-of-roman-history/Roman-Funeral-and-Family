// ----------------------------------------------------------------------------
// This source code is provided only under the Creative Commons licensing terms stated below.
// HVWC Multiplayer Platform alpha v1 by Institute for Digital Intermedia Arts at Ball State University \is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
// Based on a work at https://github.com/HVWC/HVWC.
// Work URL: http://idialab.org/mellon-foundation-humanities-virtual-world-consortium/
// Permissions beyond the scope of this license may be available at http://idialab.org/info/.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/4.0/deed.en_US.
// ----------------------------------------------------------------------------
using UnityEngine;
using System;

/// <summary>
///  This singleton class handles positioning relative to the GeographicMarker instance.
/// </summary>
public class GeographicManager : MonoBehaviour {

    #region Fields
    /// <summary>
    ///  The instance of this singleton class.
    /// </summary>
    public static GeographicManager Instance { get; private set; }
    /// <summary>
	///  The Geographic marker by which we derive the relative positioning.
	/// </summary>
    GeographicMarker geoMarker;
    #endregion

    #region Unity Messages
    /// <summary>
	/// A message called when the script instance is being loaded.
	/// </summary>
    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// A message called when the script starts.
    /// </summary>
    void Start() {
        geoMarker = FindObjectOfType<GeographicMarker>();
    }
    #endregion

    #region Methods
    /// <summary>
	/// A method that returns a position from a coordinate relative to the GeographicMarker.
	/// </summary>
    /// /// <param name="latitude">
	/// The latitude of the coordinate.
	/// </param>
	/// <param name="longitude">
	/// The longitude of the coordinate.
	/// </param>
    /// /// <param name="elevation">
	/// The elevation of the coordinate.
	/// </param>
    /// <returns>
    /// The position.
    /// </returns>
    public Vector3 GetPosition(double latitude, double longitude, double elevation) {
        if(geoMarker == null) {
            geoMarker = FindObjectOfType<GeographicMarker>();
        }
        GeographicCoord geoCoord = new GeographicCoord(GeographicCoord.Mode.LatLongDecimalDegrees);
        geoCoord.text = latitude + ", " + longitude + ", " + elevation;
		return geoMarker.Translate(geoCoord.ToGeoPoint());
    }

	public Vector3 GetCurrentMarkerMercator(double z, double x)
	{
		if(geoMarker == null) {
			geoMarker = FindObjectOfType<GeographicMarker>();
		}
		return geoMarker.GetCurrentMercator(z, x);

	}
    #endregion

}
