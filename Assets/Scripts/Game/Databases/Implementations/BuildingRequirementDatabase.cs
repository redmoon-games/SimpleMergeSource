using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "BuildingRequirementDatabase", menuName = "Scriptable Database/BuildingRequirements")]
    public class BuildingRequirementDatabase : AbstractDatabase<List<BuildingRequirementVo>>
    {
        public override string AssociatedWorksheet => "BuildingRequirements";
      
        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.BuildingRequirementsTable(spreadSheet);
        }

        public List<BuildingRequirementVo> GetByBuildingId(string buildingId) =>
            data.GetListBy(value => value.buildingId == buildingId);
    }
}