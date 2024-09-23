using Game.ResourceSystem;
using System.Collections.Generic;
using UnityEngine;

public interface IStaticDataService : IService
{
    void Load();
    PropEntity GetEntityData(string ID);
    LocationEntity GetLocationData(LocationID ID);
    SurfaceEntity GetSurfaceByMaterial(PhysicMaterial physicsMaterial);

    IEnumerable<PropEntity> GetAllEntitiesData();
    IEnumerable<ToolEntity> GetAllToolsData();
    IEnumerable<LocationEntity> GetAllLocationsData();
    IEnumerable<SalableEntity> GetAllSalableEntities();
    IEnumerable<SurfaceEntity> GetAllSurfaceEntities();

}