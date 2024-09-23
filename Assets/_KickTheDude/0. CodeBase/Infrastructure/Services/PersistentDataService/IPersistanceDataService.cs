using System;

public interface IPersistanceDataService
{
    event Action PersistanceDataChanged;

    void LoadPersistanceDataToStaticData();
    void ClearData();

    void SaveSalableEntityStatus(SalableEntity salableEntity);

    bool IsItFirstLaunch();
    void SetNotFirstLaunch();
}
