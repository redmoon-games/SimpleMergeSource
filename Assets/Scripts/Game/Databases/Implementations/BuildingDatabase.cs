using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "BuildingDatabase", menuName = "Scriptable Database/Buildings")]
    public class BuildingDatabase : AbstractDatabase<List<BuildingVo>>
    {
        public override string AssociatedWorksheet => "Buildings";
        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseBuildingsTable(spreadSheet);
        }
    }
}