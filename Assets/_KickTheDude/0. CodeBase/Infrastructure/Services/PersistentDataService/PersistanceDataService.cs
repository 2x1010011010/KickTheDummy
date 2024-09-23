using System;
using System.Collections.Generic;
using CodeBase.Services.StaticData;
using Game.ResourceSystem;
using UnityEngine;

public class PersistanceDataService : IPersistanceDataService   
{
    private const string ID_PLAYER_SELECTED_SKIN = "ID_SELECTED_PLAYER_SKIN";
    private const string ID_FIRST_LAUNCH = "ID_FIRST_LAUNCH";

    public event Action PersistanceDataChanged;

    private IStaticDataService _staticDataService;

    public PersistanceDataService(IStaticDataService staticDataService)
    {
        _staticDataService = staticDataService;
    }

    public void LoadPersistanceDataToStaticData()
    {
        LoadPersistanceSalableData();
    }

    private void LoadPersistanceSalableData()
    {
        /*
        foreach (var salableEntity in _staticDataService.GetAllSalableEntities())
        {
            salableEntity.CurentState = PlayerPrefs.GetInt(salableEntity.ID);

            if (salableEntity.SalableResourceEntity.EntityStatus == EntityStatus.Hidden) continue;

            if (salableEntity.Purchased)
                salableEntity.SalableResourceEntity.SetStatus(EntityStatus.Unlocked);
            else
                salableEntity.SalableResourceEntity.SetStatus(EntityStatus.Locked);

            Debug.Log(salableEntity.ID + " salavble data loaded!");
        }
        */
        Debug.Log("All Persistance Salable Data Loaded!");
    }

    public void SaveResourceEntityStatus(string resourceID, EntityStatus entityStatus)
    {
        PlayerPrefs.SetInt(resourceID, (int)entityStatus);
        PlayerPrefs.Save();

        LoadPersistanceDataToStaticData();
    }

    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public void SaveSalableEntityStatus(SalableEntity salableEntity)
    {
        PlayerPrefs.SetInt(salableEntity.ID, salableEntity.CurentState);
        PlayerPrefs.Save();
    }

    private void SavePrefsState()
    {
        PlayerPrefs.Save();

        PersistanceDataChanged?.Invoke();
    }

    public bool IsItFirstLaunch()
    {
        return PlayerPrefs.GetInt(ID_FIRST_LAUNCH) == 0;
    }

    public void SetNotFirstLaunch()
    {
        PlayerPrefs.SetInt(ID_FIRST_LAUNCH, 1);
        PlayerPrefs.Save();
    }
}
