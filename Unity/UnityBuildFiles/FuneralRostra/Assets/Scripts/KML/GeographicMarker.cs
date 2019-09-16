/*KML translation tool built by Tipodean Technologies. 10/31/2011  
 *For questions please contact chris@tipodean.com 
 *This code is for use by UCLA in the Humanities Virtual World Consortium 
 *
 *
*/ 

using UnityEngine;
using System;	
using System.Collections;


public class GeographicMarker : MonoBehaviour
{
	public GeographicCoord marker;
	//public double scale = 80000.0;
	
	/*
	 * Given 41°53'30.36"N, 12°29'4.87"E
	 * Guess 41°53'32.55"N, 12°29'7.74"E 41.89238, 12.48548
	 * Test 1 41.892248794671, 12.486068507438
	 * Test 2 41.892278549916, 12.485591719726
	 * Test 3 41.892320557058, 12.484961295087
	 * Test 4 41.892239638510, 12.486038806491
	 * 
	 * Column Pocas	41°53'32.79"N, 12°29'5.46"E		41.892442, 12.48485
	 * Column 1		41°53'32.62"N, 12°29'4.79"E		41.892394, 12.484664
	 * Column 7		41°53'31.67"N, 12°29'7.60"E		41.892131, 12.485444
	 * Building SE	41°53'33.99"N, 12°29'7.28"E		41.892775, 12.485356
	 * 
	 * 20m N 41°53'33.20"N, 12°29'7.74"E 41.892556, 12.485483
	 * 20m E 41°53'32.55"N, 12°29'8.61"E 41.892375, 12.485725
	 * 
	 * 41°53'32.55"N, 12°29'7.74"E 4564099.122524217, 1345708.408409107
	 * 41°53'33.20"N, 12°29'7.74"E 4564099.122524217, 1345708.408409107
	 * 41°53'32.55"N, 12°29'8.61"E 4564099.122524217, 1345708.408409107
	 * 
	 */
	
	public GeographicCoord[] testPoints;
	
	private double markerX = 0.0;
	private double markerZ = 0.0;
	//new scaleFactor code
	private double scaleFactor = 1.0;
	
	public void RefreshMarker()
	{
		GeoPoint geo = marker.ToGeoPoint();
		GeographicCoord.DecimalDegreesToMercatorSpherical(out markerZ, out markerX, geo.latitude, geo.longitude);
	}
	
	void Start()
	{
		RefreshMarker();
	}
	
	public Vector3 Translate(double latitude, double longitude, float elevation)
	{
        RefreshMarker();
		Vector3 pos = transform.position;
		
		double x, z;
		GeographicCoord.DecimalDegreesToMercatorSpherical(out z, out x, latitude, longitude);
		//new scaleFactor code
		scaleFactor = 1/Math.Cos((Math.PI/180 * latitude)); 

		pos.x += (float)((x - markerX)/scaleFactor);
		pos.y += elevation - marker.elevation;
		pos.z += (float)((z - markerZ)/scaleFactor);

//		pos.x += (float)((x - markerX)*scale);
//		pos.y += elevation - marker.elevation;
//		pos.z += (float)((z - markerZ)*scale);
		return pos;
	}

	public Vector3 GetCurrentMercator(double z, double x)
	{
		RefreshMarker();
		Vector3 pos = transform.position;

		GeoPoint geo = marker.ToGeoPoint();


		//GeographicCoord.DecimalDegreesToMercatorSpherical(out z, out x, latitude, longitude);
		//new scaleFactor code
		scaleFactor = 1/Math.Cos((Math.PI/180 * geo.latitude)); 

		pos.x = (float)(x * scaleFactor + markerX);
		pos.y += marker.elevation;
		pos.z = (float)(z * scaleFactor + markerZ);

		//		pos.x += (float)((x - markerX)*scale);
		//		pos.y += elevation - marker.elevation;
		//		pos.z += (float)((z - markerZ)*scale);
		return pos;
	}


    public Vector3 Translate(GeoPoint geoPoint)
	{
		return Translate(geoPoint.latitude, geoPoint.longitude, geoPoint.elevation);
	}





	void OnDrawGizmos()
	{
		if(testPoints == null)
			return;
		
		if((markerX == 0.0) || (markerZ == 0.0))
			RefreshMarker();
		
		Vector3 p1, p2 = transform.position;
		for(int i=0; i<testPoints.Length; ++i)
		{
			p1 = Translate(testPoints[i].ToGeoPoint());
			Gizmos.DrawSphere(p1, 1.0f);
			Gizmos.DrawLine(p1, p2);
		}
	}
}
