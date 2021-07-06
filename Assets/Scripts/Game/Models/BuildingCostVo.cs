using System;
using System.Collections.Generic;
using Core.BigNumberAsset;

namespace Game.Models
{
    [Serializable]
    public class BuildingCostVo
    {
        public string id;
        public string buildingTypeId;
        public int level;
        public long experience;
        public BigValue cost;
        public int deliveryTime;
        public int deliveryCostCrystal;
        public List<string> deliveryCostUnits;
    }
}