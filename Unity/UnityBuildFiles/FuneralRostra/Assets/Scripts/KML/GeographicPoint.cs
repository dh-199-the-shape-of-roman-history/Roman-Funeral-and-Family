/*KML translation tool built by Tipodean Technologies. 10/31/2011  
 *For questions please contact chris@tipodean.com 
 *This code is for use by UCLA in the Humanities Virtual World Consortium 
 *
 *
*/ 


using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;

[System.Serializable]
public struct GeoPoint
{
	// Stores geograpic location in decimal degrees and meters
	public double latitude;
	public double longitude;
	public float elevation;
	
	public GeoPoint(double lat, double lon)
	{
		latitude = lat;
		longitude = lon;
		elevation = 0.0f;
	}
	
	public GeoPoint(double lat, double lon, float ele)
	{
		latitude = lat;
		longitude = lon;
		elevation = ele;
	}
	
	public override string ToString()
	{
		return string.Format("{0}, {1}", latitude.ToString("g8"), longitude.ToString("g8"));
	}
	
	public string ToStringDMS()
	{
		int latD, latM, lonD, lonM;
		double latS, lonS;
		GeographicCoord.CardinalDirection latC, lonC;
		
		GeographicCoord.DecimalDegreesToDms(out latD, out latM, out latS, out latC, latitude, true);
		GeographicCoord.DecimalDegreesToDms(out lonD, out lonM, out lonS, out lonC, longitude, false);
		return string.Format("{0} {1}'{2}\"{3}, {4} {5}'{6}\"{7}",
		                     latD, latM, latS.ToString("f2"), GeographicCoord.CardinalDirectionAbbr(latC),
		                     lonD, lonM, lonS.ToString("f2"), GeographicCoord.CardinalDirectionAbbr(lonC));
	}
	
	public string ToStringMercatorElliptical()
	{
		double x, z;
		GeographicCoord.DecimalDegreesToMercatorElliptical(out z, out x, latitude, longitude);
		return string.Format("{0}, {1}", z.ToString("g"), x.ToString("g"));
	}
	
	public string ToStringMercatorSpherical()
	{
		double x, z;
		GeographicCoord.DecimalDegreesToMercatorSpherical(out z, out x, latitude, longitude);
		return string.Format("{0}, {1}", z.ToString("g"), x.ToString("g"));
	}
}
