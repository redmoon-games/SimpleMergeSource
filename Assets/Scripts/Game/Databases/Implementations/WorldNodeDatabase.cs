using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "WorldNodeDatabase", menuName = "Scriptable Database/WorldNode")]
    public class WorldNodeDatabase : AbstractDatabase<List<WorldNodeVo>>
    {
        public override string AssociatedWorksheet => "WorldNodes";
        
        public WorldNodeVo GetNodeById(string id) => 
            data.GetBy(value => value.id == id);

        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseWorldNodeTable(spreadSheet);
        }
    }
}
