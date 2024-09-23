using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.StaticData;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ServicesInstaller : MonoInstaller
{
    [field: SerializeField, BoxGroup("SETUP")] public ApplicationService ApplicationService { get; private set; }
    [field: SerializeField, BoxGroup("SETUP")] public LocationsService LocationsService { get; private set; }

    [field: SerializeField, BoxGroup("PARAMETERS")] public bool DisableAnalytics { get; private set; }

    public override void InstallBindings()
    {
        BindApplicationService();
        BindInputService();
        BindTimeService();
        BindAnalyticsService();
        BindAdvertisingService();
        BindGameSettngsService();
        BindAssetProviderService();
        BindStaticDataService();
        BindPersistanceDataService();
        BindLocationsService();
    }

    private void BindApplicationService()
    {
        Container.Bind<IApplicationService>().FromInstance(ApplicationService).AsSingle().NonLazy();
    }

    private void BindInputService()
    {
        var inputService = new InputService();
        Container.Bind<IInputService>().FromInstance(inputService).AsSingle().NonLazy();
    }

    private void BindAdvertisingService()
    {
        Container.Bind<IAdvertisingService>().FromInstance(new DummyAdvertisingService()).AsSingle().NonLazy();
    }

    private void BindAnalyticsService()
    {
        if (DisableAnalytics)
        {
            Container.Bind<IAnalyticsService>().FromInstance(new DummyAnalyticsService()).AsSingle().NonLazy();
        }
        else
        {
            var analytics = new List<IAnalyticsEventSender>() { new AppmetricaEventSender() };
            var analyticsService = new AnalyticsService(analytics);

            analyticsService.Initialize();

            Container.Bind<IAnalyticsService>().FromInstance(analyticsService).AsSingle().NonLazy();
        }
    }

    private void BindTimeService()
    {
        var timeService = new TimeService();
        Container.Bind<ITimeService>().FromInstance(timeService).AsSingle().NonLazy();
    }

    private void BindGameSettngsService()
    {
        var gameSettingsService = new GameSettingsService();
        Container.Bind<GameSettingsService>().FromInstance(gameSettingsService).AsSingle().NonLazy();
    }

    private void BindAssetProviderService()
    {
        var resourcesService = new AssetProviderService();
        Container.Bind<IAssetProvider>().To<AssetProviderService>().FromInstance(resourcesService).AsSingle().NonLazy();

        resourcesService.Initialize();
    }

    private void BindStaticDataService()
    {
        var staticDataService = new StaticDataService();
        staticDataService.Load();

        Container.Bind<IStaticDataService>().To<StaticDataService>().FromInstance(staticDataService).AsSingle();
    }

    private void BindPersistanceDataService()
    {
        Container.Bind<IPersistanceDataService>().To<PersistanceDataService>().FromNew().AsSingle().NonLazy();
    }

    private void BindLocationsService()
    {
        Container.Bind<ILocationsService>().FromInstance(LocationsService).AsSingle().NonLazy();
    }
}
