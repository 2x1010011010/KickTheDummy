using CodeBase.Services.StaticData;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class UIMainMenuBootstraper : UIScenesBootstraper
{
    [SerializeField] private Camera _camera;

    private ApplicationService _applicationService;
    private IUIService _uiService;

    private IPersistanceDataService _persistanceDataService;
    private IAnalyticsService _analyticsService;
    private ITestingService _testingService;
    private IStaticDataService _staticDataService;

    [Inject]
    private void Construct(IUIService uiService, IPersistanceDataService persistanceDataService, IAnalyticsService analyticsService, ITestingService testingService, IStaticDataService staticDataService, ApplicationService applicationService)
    {
        _applicationService = applicationService;
        _uiService = uiService;
        _persistanceDataService = persistanceDataService;
        _testingService = testingService;
        _staticDataService = staticDataService;

        _analyticsService = analyticsService;
    }

    private void Awake()
    {
        _camera.enabled = false;
    }

    public async override void Bootstrap()
    {

    }

    [Button(ButtonSizes.Large)]
    private void DropAllData()
    {
        _testingService.DropAllData();
    }
}
