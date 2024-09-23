using System.Collections;
using UnityEngine;
using Zenject;

public class Bootstraper : MonoBehaviour
{
    private ILocationsService _locationsService;
    private IUIService _uiService;
    private IAnalyticsService _analyticsService;
    private IAdvertisingService _advertisingService;

    [Inject]
    private void Construct(ILocationsService locationsService, IUIService uiService, IAnalyticsService analyticsService, IAdvertisingService advertisingService)
    {
        _locationsService = locationsService;
        _uiService = uiService;
        _analyticsService = analyticsService;
        _advertisingService = advertisingService;

        Debug.Log("[BOOTSTRAPER] Game bootstraper get dependensies!");
    }

    private void Start()
    {
        StartCoroutine(Starting());
    }

    private IEnumerator Starting()
    {
        _uiService.OpenWindow<UILoadingWindow>();
        yield return new WaitForSeconds(2f);

        _locationsService.LoadLocationByID(LocationID.MainMenu);

        Application.targetFrameRate = 9999;
    }
}
