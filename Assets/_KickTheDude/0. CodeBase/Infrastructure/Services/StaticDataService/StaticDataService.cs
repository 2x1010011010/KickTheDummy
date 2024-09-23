using System.Collections.Generic;
using System.Linq;
using Game.ResourceSystem;
using UnityEngine;

namespace CodeBase.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string RESOURCE_ENTITIES_DATA_PATH = "StaticData/Props";
        private const string LOCATIONS_ENTITIES_DATA_PATH = "StaticData/Locations";
        private const string SURFACES_ENTITIES_DATA_PATH = "StaticData/Surfaces";
        private const string TOOLS_DATA_PATH = "StaticData/Tools";

        private Dictionary<string, PropEntity> _entities;
        private Dictionary<LocationID, LocationEntity> _levels;
        private Dictionary<string, SalableEntity> _salableEntities;
        private List<PhysicalConnectionParameters> _physicalConnectionParameters;
        private List<ToolEntity> _toolEntities;
        private List<SurfaceEntity> _surfaceEntities;

        public void Load()
        {
            _entities = Resources
                .LoadAll<PropEntity>(RESOURCE_ENTITIES_DATA_PATH)
                .ToDictionary(x => x.ID, x => x);

            _levels = new Dictionary<LocationID, LocationEntity>(Resources
                .LoadAll<LocationEntity>(LOCATIONS_ENTITIES_DATA_PATH)
                .ToDictionary(x => x.LocationID, x => x));

            _toolEntities = Resources
                .LoadAll<ToolEntity>(TOOLS_DATA_PATH)
                .ToList();

            _surfaceEntities = Resources
                .LoadAll<SurfaceEntity>(SURFACES_ENTITIES_DATA_PATH)
                .ToList();
        }

        public PropEntity GetEntityData(string id) =>
            _entities.TryGetValue(id, out PropEntity staticData)
                ? staticData
                : null;

        public LocationEntity GetLocationData(LocationID id) =>
            _levels.TryGetValue(id, out LocationEntity staticData)
                ? staticData
                : null;

        public IEnumerable<LocationEntity> GetAllLocationsData()
            => _levels.Values.OrderBy(x => x.name).ToList();

        public IEnumerable<PropEntity> GetAllEntitiesData()
            => _entities.Values.ToList();

        public IEnumerable<SalableEntity> GetAllSalableEntities()
            => _salableEntities.Values.ToList();

        public IEnumerable<ToolEntity> GetAllToolsData()
            => _toolEntities;

        public SurfaceEntity GetSurfaceByMaterial(PhysicMaterial physicsMaterial)
            => _surfaceEntities.FirstOrDefault(x => x.PhysicMaterial.name == physicsMaterial.name);

        public IEnumerable<SurfaceEntity> GetAllSurfaceEntities()
            => _surfaceEntities;
    }
}