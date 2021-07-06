using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "BuildingTypeDatabase", menuName = "Scriptable Database/BuildingType")]
    public class BuildingTypeDatabase : AbstractDatabase<List<BuildingTypeVo>>
    {
        public override string AssociatedWorksheet => "BuildingTypes";
        
        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseBuildingTypeTable(spreadSheet);
        }
    }
}