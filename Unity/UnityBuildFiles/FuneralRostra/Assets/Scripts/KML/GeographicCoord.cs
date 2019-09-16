/*KML translation tool built by Tipodean Technologies. 10/31/2011  
 *For questions please contact chris@tipodean.com 
 *This code is for use by UCLA in the Humanities Virtual World Consortium 
 *
 *
*/ 
 

using UnityEngine;
using System;
using System.Collections;
using System.Text.RegularExpressions;

[System.Serializable]
public class GeographicCoord
{
	public enum CardinalDirection
	{
		East,
		North,
		South,
		West
	}

	public enum Mode
	{
		LatLongDecimalDegrees,
		LatLongDMS,
		MercatorElliptical,
		MercatorSpherical
	}

	public const double RaduisMax = 6378137.0;
    public const double RaduisMin = 6356752.3142;
    public const double RaduisRatio = RaduisMin / RaduisMax;
 
    public const double Deg2Rad = Math.PI / 180.0;
    public const double Rad2Deg = 180.0 / Math.PI;
    public const double Pi2 = Math.PI / 2.0;	

    public static readonly double Eccent = Math.Sqrt(1.0 - (RaduisRatio * RaduisRatio));
    public static readonly double Com = 0.5 * Eccent;

	// Geographic coordinate in comma seperated text form (eg. 41 53'32.79"N, 12 29'5.46"E in DMS)
	public string text = "";
	// Geographic coordinate format (eg. DMS)
	public Mode mode = Mode.LatLongDMS;
	// Elevation above sea level in meters
	public float elevation = 0.0f;
	
	public static string CardinalDirectionAbbr(CardinalDirection cardinalDirection)
	{
		switch(cardinalDirection)
		{
		case CardinalDirection.East:
			return "E";
		case CardinalDirection.North:
			return "N";
		case CardinalDirection.South:
			return "S";
		case CardinalDirection.West:
			return "W";
		}
		return "";
	}

	public static double DmsToDecimalDegrees(int Degrees, int Minutes, double Seconds, CardinalDirection Direction)
	{
		double deg = (double)Degrees + ((double)Minutes * 60.0 + Seconds) / 3600.0;
		
		if((Direction == CardinalDirection.South) || (Direction == CardinalDirection.West))
			return -deg;
		return deg;
	}
	
	public static void DecimalDegreesToDms(out int Degrees, out int Minutes, out double Seconds, double DecimalDegrees)
	{
		if(DecimalDegrees < 0.0)
			DecimalDegrees = -DecimalDegrees;
		Degrees = Mathf.FloorToInt((float)DecimalDegrees);
		double decimalMinutes = 60.0 * (DecimalDegrees - (float)Degrees);
		Minutes = Mathf.FloorToInt((float)decimalMinutes);
		Seconds = 60.0 * (decimalMinutes - (double)Minutes);
	}

	public static void DecimalDegreesToDms(out int Degrees, out int Minutes, out double Seconds, out CardinalDirection Direction, double DecimalDegrees, bool isLatitude)
	{
		DecimalDegreesToDms(out Degrees, out Minutes, out Seconds, DecimalDegrees);
		
		if(isLatitude)
		{
			if(DecimalDegrees < 0.0)
			{
				Direction = CardinalDirection.South;
				return;
			}
			Direction = CardinalDirection.North;
			return;
		}
		if(DecimalDegrees < 0.0)
		{
			Direction = CardinalDirection.West;
			return;
		}
		Direction = CardinalDirection.East;
		return;
	}
	
    public static void DecimalDegreesToMercatorElliptical(out double z, out double x, double latitude, double longitude)
    {
		double lat = Math.Min(89.5, Math.Max(latitude, -89.5));
		double phi = lat * Deg2Rad;
		double con = Eccent * Math.Sin(phi);
		con = Math.Pow(((1.0 - con) / (1.0 + con)), Com);
		z = 0.0 - RaduisMax * Math.Log(Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con);
		x = RaduisMax * (longitude * Deg2Rad);
	}

	public static void DecimalDegreesToMercatorSpherical(out double z, out double x, double latitude, double longitude)
	{
		double lat = Math.Min(89.5, Math.Max(latitude, -89.5));
		double phi = lat * Deg2Rad;
		double con = Eccent * Math.Sin(phi);
		con = Math.Pow(((1.0 - con) / (1.0 + con)), Com);
		z = 0.0 - RaduisMax * Math.Log(Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con);
		x = RaduisMax * (longitude * Deg2Rad);
	}	
	
//	public static void DecimalDegreesToMercatorSpherical(out double z, out double x, double latitude, double longitude)
//	{
//		z = (float)(Rad2Deg * Math.Log(Math.Tan(Math.PI/4.0 + latitude*Deg2Rad/2.0)));
//		x = (float)(Rad2Deg * Math.Log(Math.Tan(Math.PI/4.0 + longitude*Deg2Rad/2.0)));
//	}
// 
	public static void MercatorEllipticalToDecimalDegrees(out double latitude, out double longitude, double z, double x)
	{
		double ts = Math.Exp(-z / RaduisMax);
		double phi = Pi2 - 2.0 * Math.Atan(ts);
		double dphi = 1.0;
		int i = 0;
		while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
		{
			double con = Eccent * Math.Sin(phi);
			dphi = Pi2 - 2.0 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), Com)) - phi;
			phi += dphi;
			++i;
		}
		latitude = phi * Rad2Deg;
		longitude = (x * Rad2Deg) / RaduisMax;
	}

	public static void MercatorSphericalToDecimalDegrees(out double latitude, out double longitude, double z, double x)
	{
		latitude = Rad2Deg * (2.0 * Math.Atan(Math.Exp(z * Deg2Rad)) - Math.PI/2.0);
		longitude = Rad2Deg * (2.0 * Math.Atan(Math.Exp(x * Deg2Rad)) - Math.PI/2.0);
	}
	
	public static bool ParseDms(out double DecimalDegrees, string DmsText)
	{
		Regex Parser = new Regex("^(?<deg>[-+0-9]+)[^0-9]+(?<min>[0-9]+)[^0-9]+(?<sec>[0-9.,]+)[^0-9.,ENSW]+(?<dir>[ENSW]*)$");
		
		if (DmsText.StartsWith("\"") && DmsText.EndsWith("\""))
			DmsText = DmsText.Substring(1, DmsText.Length - 2).Replace("\"\"", "\"");
		
		Match match = Parser.Match(DmsText);
		if (!match.Success)
		{
			DecimalDegrees = 0.0;
			return false;
		}
		
		DecimalDegrees = double.Parse(match.Groups["deg"].Value);
		DecimalDegrees += double.Parse(match.Groups["min"].Value) / 60;
		DecimalDegrees += double.Parse(match.Groups["sec"].Value) / 3600;
		if (match.Groups["dir"].Success)
		{
			char ch = match.Groups["dir"].Value[0];
			if ((ch == 'S') || (ch == 'W'))
				DecimalDegrees = -DecimalDegrees;
		}
		return true;
	}
	
	public GeographicCoord(Mode coordSystem)
	{
		mode = coordSystem;
	}
	
	public bool Convert(ref GeographicCoord target)
	{
		GeoPoint geo = ToGeoPoint();
		switch(target.mode)
		{
		case Mode.MercatorElliptical:
			target.text = geo.ToStringMercatorElliptical();
			return true;
		case Mode.MercatorSpherical:
			target.text = geo.ToStringMercatorSpherical();
			return true;
		case Mode.LatLongDecimalDegrees:
			target.text = geo.ToString();
			return true;
		case Mode.LatLongDMS:
			target.text =  geo.ToStringDMS();
			return true;
		}
		target.text = "";
		return false;
	}
	
	public GeoPoint ToGeoPoint()
	{
		GeoPoint geo = new GeoPoint();
		
		string[] input = text.Split(',');
		
		if(input.Length > 1)
		{
			switch(mode)
			{
			case Mode.MercatorElliptical:
				MercatorEllipticalToDecimalDegrees(out geo.latitude, out geo.longitude, double.Parse(input[0].Trim()), double.Parse(input[1].Trim()));
				break;
			case Mode.MercatorSpherical:
				MercatorSphericalToDecimalDegrees(out geo.latitude, out geo.longitude, double.Parse(input[0].Trim()), double.Parse(input[1].Trim()));
				break;
			case Mode.LatLongDecimalDegrees:
				geo.latitude = double.Parse(input[0].Trim());
				geo.longitude = double.Parse(input[1].Trim());
				break;
			case Mode.LatLongDMS:
				ParseDms(out geo.latitude, input[0].Trim());
				ParseDms(out geo.longitude, input[1].Trim());
				break;
			}
			
			if (input.Length > 2)
				geo.elevation = float.Parse(input[2].Trim());
			else
				geo.elevation = elevation;
		}
		
		return geo;
	}
	
	public override string ToString()
	{
		return text;
	}
}
