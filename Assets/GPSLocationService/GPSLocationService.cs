using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GPSLocationServiceState : int
{
    Stopped = 0,
    Started,
    Failed,
    Disabled,
    TimeOut,
}

public class GPSLocationServiceEventArgs : EventArgs
{
    public GPSLocationServiceState State { get; private set; }
    public string StateDescription{ get; private set; }
    public float Latitude { get; private set; }
    public float Longitude { get; private set; }
    public float Altitude { get; private set; }
    public float HorizontalAccuracy { get; private set; }
    public float VerticalAccuracy { get; private set; }
    public float TimeStamp { get; private set; }

    public GPSLocationServiceEventArgs(GPSLocationServiceState s, string desc, float lat, float lon, float alt, float Hacc, float Vacc, float stamp)
    {
        this.State = s;
        this.StateDescription = desc;
        this.Latitude = lat;
        this.Longitude = lon;
        this.Altitude = alt;
        this.HorizontalAccuracy = Hacc;
        this.VerticalAccuracy = Vacc;
        this.TimeStamp = stamp;
    }
}

public class GPSLocationService : MonoBehaviour {

    public bool IsStart { get; set; }

    public float Latitude { get; private set; }
    public float Longitude { get; private set; }
    public float Altitude { get; private set; }
    public float HorizontalAccuracy { get; private set; }
    public float VerticalAccuracy { get; private set; }
    public float TimeStamp { get; private set; }

    public event EventHandler<GPSLocationServiceEventArgs> OnServiceReport;
        
    private void Awake()
    {
        IsStart = false;
        Latitude = -1;
        Longitude = -1;
        Altitude = -1;
        HorizontalAccuracy = -1;
        VerticalAccuracy = -1;
        TimeStamp = -1;
    }

    private void OnDestroy()
    {
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }

    public void StartGPSLocationService()
    {
        if (IsStart)
        {
            Debug.Log("Already start gps location service!");
            if (OnServiceReport != null)
            {
                OnServiceReport(this, new GPSLocationServiceEventArgs(GPSLocationServiceState.Started, "Already start gps location service!",
                    Latitude, Longitude, Altitude, HorizontalAccuracy, VerticalAccuracy, TimeStamp));
            }
            return;
        }
        
        StartCoroutine(CoStartGPSLocationService());
    }

    public void StopGPSLocationService()
    {                
        StartCoroutine(CoStopGPSLocationService());                
    }

    private IEnumerator CoStartGPSLocationService()
    {
        IsStart = true;

        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS location service not enabled by user");
            IsStart = false;
            if (OnServiceReport != null)
            {
                OnServiceReport(this, new GPSLocationServiceEventArgs(GPSLocationServiceState.Disabled, "GPS location service not enabled by user",
                    Latitude, Longitude, Altitude, HorizontalAccuracy, VerticalAccuracy, TimeStamp));
            }
            yield break;
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            IsStart = false;
            Debug.Log("Timed out");
            if (OnServiceReport != null)
            {
                OnServiceReport(this, new GPSLocationServiceEventArgs(GPSLocationServiceState.TimeOut, "GPS location service initialize time out",
                    Latitude, Longitude, Altitude, HorizontalAccuracy, VerticalAccuracy, TimeStamp));
            }
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            IsStart = false;
            Debug.Log("Unable to determine device location");
            if (OnServiceReport != null)
            {
                OnServiceReport(this, new GPSLocationServiceEventArgs(GPSLocationServiceState.Failed, "Unable to determine device location",
                    Latitude, Longitude, Altitude, HorizontalAccuracy, VerticalAccuracy, TimeStamp));
            }
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Latitude = Input.location.lastData.latitude;
            Longitude = Input.location.lastData.longitude;
            Altitude = Input.location.lastData.altitude;
            HorizontalAccuracy = Input.location.lastData.horizontalAccuracy;
            VerticalAccuracy = Input.location.lastData.verticalAccuracy;

            if (OnServiceReport != null)
            {
                OnServiceReport(this, new GPSLocationServiceEventArgs(GPSLocationServiceState.Started, "Unable to determine device location",
                    Latitude, Longitude, Altitude, HorizontalAccuracy, VerticalAccuracy, TimeStamp));
            }

            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
    }

    private IEnumerator CoStopGPSLocationService()
    {
        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();

        // Wait until service stopped
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Stopped && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        IsStart = false;
    }

    /**
     * 두 지점간의 거리 계산
     * http://fruitdev.tistory.com/189
     * @param lat1 지점 1 위도
     * @param lon1 지점 1 경도
     * @param lat2 지점 2 위도
     * @param lon2 지점 2 경도
     * @param unit 거리 표출단위(0:Kilometer, 1:Meter, 2:Mile)
     * @return
     */
    public double GPSDistance(double lat1, double lon1, double lat2, double lon2, int distanceType)
    {

        double theta = lon1 - lon2;
        double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));

        dist = Math.Acos(dist);
        dist = rad2deg(dist);
        dist = dist * 60 * 1.1515;

        if (distanceType == 0)
        {
            // kilometer
            dist = dist * 1.609344;
        }
        else if (distanceType == 1)
        {
            // meter
            dist = dist * 1609.344;
        }

        return (dist);
    }

    // This function converts decimal degrees to radians
    public double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    // This function converts radians to decimal degrees
    public double rad2deg(double rad)
    {
        return (rad * 180 / Math.PI);
    }
}
