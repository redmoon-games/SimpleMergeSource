using System;

namespace Game.Models
{
    [Serializable]
    public class BuildingVo
    {
        public string id;
        public string buildingTypeId;
        public int level;
        public string image;
        public string nameId;
        public string descriptionId;
    }
}