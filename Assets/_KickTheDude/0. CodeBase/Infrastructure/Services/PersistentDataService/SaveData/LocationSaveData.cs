using System;
using System.Collections.Generic;

[Serializable]
public class LocationSaveData
{
    public int SaveDataType = 1;
    public bool IsAutosave;
    public string LocationResourceID;
    public string SaveName;
    public List<CharacterSaveData> CharacterSaveDatas = new List<CharacterSaveData>();
    public List<InteractableObjectSaveData> InteractableObjectsSaveDatas = new List<InteractableObjectSaveData>();
    public List<VehicleSaveData> VehicleSaveData = new List<VehicleSaveData>();
}
