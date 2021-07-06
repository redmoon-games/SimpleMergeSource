using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "BuildingCostDatabase", menuName = "Scriptable Database/BuildingCosts")]
    public class BuildingCostDatabase : AbstractDatabase<List<BuildingCostVo>>
    {
        public override string AssociatedWorksheet => "BuildingCosts";

        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseBuildingCostsTable(spreadSheet);
        }

        public BuildingCostVo GetByTypeAndLevel(string typeId, int level) =>
            data.GetBy(value => value.buildingTypeId == typeId && value.level == level);
    }
}