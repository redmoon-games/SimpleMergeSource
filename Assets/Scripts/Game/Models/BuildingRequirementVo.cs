using System;

namespace Game.Models
{
    [Serializable]
    public class BuildingRequirementVo
    {
        public string id;
        public string buildingId;
        public string requiredBuildingId;
    }
}