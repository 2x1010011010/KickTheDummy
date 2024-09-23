using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Services.StaticData;
using Game.InteractiveSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Zenject;

public class LocationsService : MonoBehaviour, ILocationsService
{
    public LocationEntity CurrentLocation { get; private set; }
    public IEnumerable<LocationEntity> AllLocationsData => _staticDataService.GetAllLocationsData();

    private ITimeService _timeService;
    private IStaticDataService _staticDataService;
    private IAssetProvider _assetsProvider;

    public LocationSaveData LoadFromSave;

    [Inject]
    public void Construct(ITimeService timeService, IStaticDataService staticDataService, IAssetProvider assetsProvider)
    {
        _timeService = timeService;
        _staticDataService = staticDataService;
        _assetsProvider = assetsProvider;
    }

    public void LoadLocationByID(LocationID ID)
    {
        var targetLocation = _staticDataService.GetLocationData(ID);

        if (targetLocation == null) return;

        Debug.Log($"[LOCATION SERVICE] Found location for load: {targetLocation.LocationID}");

        LoadLocation(targetLocation);
    }

    public void ReloadLocation()
    {
        LoadLocation(CurrentLocation);
    }

    private void LoadLocation(LocationEntity locationEntity, Action actionAfterLoad = null)
    {
        if (locationEntity == null) return;

        _timeService.ResetTimeValue();

        StartCoroutine(Loading(locationEntity, actionAfterLoad));

        CurrentLocation = locationEntity;
    }

    private IEnumerator Loading(LocationEntity locationEntity, Action action)
    {
        AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(locationEntity.SceneAssetReference, LoadSceneMode.Single, false);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            yield return handle.Result.ActivateAsync();

        GC.Collect();
        Resources.UnloadUnusedAssets();

        action?.Invoke();
    }
}
