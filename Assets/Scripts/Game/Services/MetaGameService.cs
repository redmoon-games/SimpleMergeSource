using System.Collections.Generic;
using System.Linq;
using Core.BigNumberAsset;
using Core.Databases;
using Game.Databases;
using Game.Databases.Implementations;
using Game.Models;
using Game.UI.Popups.BuildingGame;
using ModestTree;
using UnityEngine;
using Zenject;

namespace Game.Services
{

    public class BuildingTypeDto
    {
        public BuildingTypeVo BuildingTypeVo;
    }
    
    public class BuildingDto
    {
        public BuildingTypeDto Type;
        public BuildingVo BuildingVo;
        public Sprite Sprite;
        public Vector2 Position;
        public readonly HashSet<BuildingDto> RequiredBuildings = new HashSet<BuildingDto>();
        public BigValue Cost;
        public int DeliveryCost;
        public long Experience;
        public int TimeToDeliver;
        public List<string> UnitIds;
    }
    
    public class MetaGameService : MonoBehaviour
    {
        private RoomSlotDatabase _roomSlotDatabase;
        private BuildingCostDatabase _buildingCostDatabase;
        private BuildingDatabase _buildingDatabase;
        private BuildingRequirementDatabase _buildingRequirementDatabase;
        private BuildingTypeDatabase _buildingTypeDatabase;
        private ResourceDatabase _resourceDatabase;
        private LocalizationDatabase _localizationDatabase;

        private readonly Dictionary<string, BuildingDto> _buildingsCache = new Dictionary<string, BuildingDto>();
        private readonly Dictionary<string, BuildingTypeDto> _buildingTypesCache = new Dictionary<string, BuildingTypeDto>();
        
        [Inject]
        public void Construct(
            RoomSlotDatabase roomSlotDatabase,
            BuildingCostDatabase buildingCostDatabase,
            BuildingDatabase buildingDatabase,
            BuildingRequirementDatabase buildingRequirementDatabase,
            BuildingTypeDatabase buildingTypeDatabase,
            ResourceDatabase resourceDatabase,
            LocalizationDatabase localizationDatabase)
        {
            _roomSlotDatabase = roomSlotDatabase;
            _buildingCostDatabase = buildingCostDatabase;
            _buildingDatabase = buildingDatabase;
            _buildingRequirementDatabase = buildingRequirementDatabase;
            _buildingTypeDatabase = buildingTypeDatabase;
            _resourceDatabase = resourceDatabase;
            _localizationDatabase = localizationDatabase;
        }

        public List<TaskData> GetAvailableTasks(List<BuildingData> currentBuildings)
        {
            var result = new List<BuildingDto>();
            var existingIds = GetIdSet(currentBuildings);
            var existingSet = GetBuildingSet(currentBuildings);
            
            foreach (var building in _buildingsCache)
            {
                if (!existingIds.Contains(building.Key) && (building.Value.RequiredBuildings.IsEmpty() || CheckRequiredBuildings(building.Value, existingSet))) 
                    result.Add(building.Value);
            }

            return ConvertToTaskData(result);
        }

        private HashSet<BuildingDto> GetBuildingSet(IEnumerable<BuildingData> currentBuildings)
        {
            var result = new HashSet<BuildingDto>();
            foreach (var building in currentBuildings)
            {
                result.Add(_buildingsCache[building.buildingId]);
            }

            return result;
        }

        private static bool CheckRequiredBuildings(BuildingDto building, HashSet<BuildingDto> currentBuildings)
        {
            foreach (var requiredBuilding in building.RequiredBuildings)
            {
                if (!currentBuildings.Contains(requiredBuilding)) return false;
            }

            return true;
        }

        public BuildingVo GetBuildingVoById(string buildingId)
        {
            return _buildingsCache[buildingId].BuildingVo;
        }

        public BuildingDto GetBuildingInfo(string buildingId) => _buildingsCache[buildingId];

        public int GetDeliveryTime(string buildingId)
        {
            return _buildingsCache[buildingId].TimeToDeliver;
        }

        private void Awake()
        {
            CacheBuildingTypes();
            CacheBuildings();
        }

        private void CacheBuildingTypes()
        {
            foreach (var typeVo in _buildingTypeDatabase.GetData())
            {
                var type = new BuildingTypeDto
                {
                    BuildingTypeVo = typeVo
                };
                _buildingTypesCache.Add(typeVo.id, type);
            }
        }

        private void CacheBuildings()
        {
            foreach (var buildingVo in _buildingDatabase.GetData())
            {
                var costs = _buildingCostDatabase.GetByTypeAndLevel(buildingVo.buildingTypeId, buildingVo.level);
                var building = new BuildingDto
                {
                    Type = _buildingTypesCache[buildingVo.buildingTypeId],
                    BuildingVo = buildingVo,
                    Cost = costs.cost,
                    DeliveryCost = costs.deliveryCostCrystal,
                    Sprite = _resourceDatabase.GetBuildingImage(buildingVo.image),
                    Position = _roomSlotDatabase.GetByBuildingType(buildingVo.buildingTypeId).GetPosition(),
                    Experience = _buildingCostDatabase.GetByTypeAndLevel(buildingVo.buildingTypeId, buildingVo.level).experience,
                    TimeToDeliver = _buildingCostDatabase.GetByTypeAndLevel(buildingVo.buildingTypeId, buildingVo.level).deliveryTime,
                    UnitIds = costs.deliveryCostUnits
                };
                
                _buildingsCache.Add(buildingVo.id, building);
            }

            UpdateRequiredBuildings();
        }

        private void UpdateRequiredBuildings()
        {
            foreach (var building in _buildingsCache.Values)
            {
                foreach (var requiredBuilding in _buildingRequirementDatabase.GetByBuildingId(building.BuildingVo.id))
                {
                    building.RequiredBuildings.Add(_buildingsCache[requiredBuilding.requiredBuildingId]);
                }
            }
        }

        private HashSet<string> GetIdSet(IEnumerable<BuildingData> source)
        {
            var result = new HashSet<string>();
            foreach (var buildingData in source)
            {
                result.Add(buildingData.buildingId);
            }

            return result;
        }

        private List<TaskData> ConvertToTaskData(IEnumerable<BuildingDto> source)
        {
            var result = new List<TaskData>();
            foreach (var buildingData in source)
            {
                var taskData = ConvertToTaskData(buildingData);
                result.Add(taskData);
            }

            return result;
        }

        private TaskData ConvertToTaskData(BuildingDto buildingData)
        {
            return new TaskData
            {
                BuildingId = buildingData.BuildingVo.id,
                Cost = buildingData.Cost,
                DeliveryCost = buildingData.DeliveryCost,
                Experience = buildingData.Experience,
                TaskDescription = _localizationDatabase.GetTextInCurrentLocale(buildingData.BuildingVo.descriptionId),
                TaskName = _localizationDatabase.GetTextInCurrentLocale(buildingData.BuildingVo.nameId),
                TimeToDeliver = buildingData.TimeToDeliver,
                ItemSprite = buildingData.Sprite,
                UnitSprites = _resourceDatabase.GetUnitSpritesByIds(buildingData.UnitIds),
                UnitIds = buildingData.UnitIds
            };
        }

        public TaskData GetTaskData(string buildingId) => ConvertToTaskData(GetBuildingInfo(buildingId));

        public List<BuildingDto> FilterTopLevelBuildings(IEnumerable<BuildingData> buildings)
        {
            var dtoList = ConvertToDtoList(buildings);

            var result = new Dictionary<string, BuildingDto>();
            
            foreach (var buildingDto in dtoList)
            {
                var buildingTypeId = buildingDto.Type.BuildingTypeVo.id;
                if (!result.ContainsKey(buildingTypeId) ||
                    result.ContainsKey(buildingTypeId) && 
                    result[buildingTypeId].BuildingVo.level < buildingDto.BuildingVo.level)
                {
                    result[buildingTypeId] = buildingDto;
                }
            }

            return result.Values.ToList();
        }

        private List<BuildingDto> ConvertToDtoList(IEnumerable<BuildingData> buildings)
        {
            var dtoList = new List<BuildingDto>();

            foreach (var building in buildings)
            {
                dtoList.Add(GetBuildingInfo(building.buildingId));
            }

            return dtoList;
        }
    }
}