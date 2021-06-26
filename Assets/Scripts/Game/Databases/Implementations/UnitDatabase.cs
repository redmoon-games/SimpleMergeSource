using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "UnitDatabase", menuName = "Scriptable Database/Unit")]
    public class UnitDatabase : AbstractDatabase<List<UnitVo>>
    {
        public override string AssociatedWorksheet => "Units";
        
        public UnitVo GetNodeById(string id) => 
            data.GetBy(value => value.id == id);
        
        public UnitVo GetNodeByMergeId(string id) => 
            data.GetBy(value => value.mergeNodeId == id);

        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = DatabaseHelpers.ParseUnitTable(ss);
        }
    }
}
