using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITestingService : IService
{
    void DropAllData();
    void ResetInterCD();
    void ResetAdsCD();
    void ResetTasks();
    void Get100000Money();
    void ResetMoney();
    void ResetAllLockedItems();
    void ReloadLocation();
    void ClearLocationSaves();
    void OpenAllLockedItems();
    void ResetPlayerSkin();
    void SetWorkshopTestMode(bool testMode);
}
