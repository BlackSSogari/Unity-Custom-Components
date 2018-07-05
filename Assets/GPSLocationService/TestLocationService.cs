using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestLocationService : MonoBehaviour {

    public GPSLocationService locationService;
	public Text m_ResultText;
	public Text m_DistanceText;

    private void Start()
    {
        if (locationService != null)
        {
            locationService.OnServiceReport += LocationService_OnServiceReport;
            locationService.StartGPSLocationService();
        }
    }

    private void LocationService_OnServiceReport(object sender, GPSLocationServiceEventArgs e)
    {
        PrintResult(e.StateDescription);
    }

    private void OnDisable()
    {
        if (locationService != null)
        {
            locationService.StopGPSLocationService();
        }
    }

    private void PrintResult(string str)
	{
		Debug.LogFormat("Result : {0}", str);
		if (m_ResultText != null)
		{
			m_ResultText.text = str;
		}
	}

    // 샘플 거리 측정
	public void OnClick_Distance()
	{
        if (locationService == null)
            return;
                
		// 마일(Mile) 단위
		double distanceMile =
            locationService.GPSDistance(37.50531, 127.0515, 37.491352, 127.011986, 2);

		// 미터(Meter) 단위
		double distanceMeter =
            locationService.GPSDistance(37.50531, 127.0515, 37.491352, 127.011986, 1);

		// 킬로미터(Kilo Meter) 단위
		double distanceKiloMeter =
            locationService.GPSDistance(37.50531, 127.0515, 37.491352, 127.011986, 0);

		m_DistanceText.text = string.Format("{0:F2}Mile, {1:F2}Meter, {2:F2}Km", distanceMile, distanceMeter, distanceKiloMeter);
	}

	
}
