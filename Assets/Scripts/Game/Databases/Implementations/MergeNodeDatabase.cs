using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Enums;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "MergeNodeDatabase", menuName = "Scriptable Database/MergeNode")]
    public class MergeNodeDatabase : AbstractDatabase<List<MergeNodeVo>>
    {
        public override string AssociatedWorksheet => "MergeNodes";
        
        public MergeNodeVo GetNodeByCategory(EMergeCategory category) => 
            data.GetBy(value => value.category == category);
        
        public MergeNodeVo GetNodeById(string id) => 
            data.GetBy(value => value.id == id);

        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = DatabaseHelpers.ParseMergeNodeTable(ss);
        }
    }
}
