using System;
using Data.AdditionalData.Values;
using UnityEngine;

namespace Infrastructure.Services
{
  public class SavedService
  {
    private readonly SavedData _data;

    public SavedService(SavedData savedData)
    {
      _data = savedData;
    }

    public void LoadProgress()
    {
      try
      {
        var savedString = PlayerPrefs.GetString(Constance.SAVE_PROGRESS_KEY);
        var loadData = JsonUtility.FromJson<SavedData>(savedString);
        //Logg.ColorLog($"LOAD PROGRESS: Data -> {savedString}", ColorType.Olive);
        if (loadData == null) return;
        _data.LoadData(loadData);
      }
      catch (Exception e)
      {
        //Logg.ColorLog("SavedData: LoadProgress " + e, LogStyle.Error);
      }

      //Logg.ColorLog($"LOAD PROGRESS level {_data.GameLevel}.{_data.MapLevel}", ColorType.Olive);
    }

    [Button]
    public void SaveProgress()
    {
      var stringData = JsonUtility.ToJson(_data);
      PlayerPrefs.SetString(Constance.SAVE_PROGRESS_KEY, stringData);
      //Logg.ColorLog("SAVE PROGRESS", ColorType.Olive);
    }

    [Button]
    public void DeleteProgress()
    {
      PlayerPrefs.DeleteAll();
    }
  }
}