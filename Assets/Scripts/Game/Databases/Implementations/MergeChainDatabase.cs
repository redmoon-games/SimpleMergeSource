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
    [CreateAssetMenu(fileName = "MergeChainDatabase", menuName = "Scriptable Database/MergeChain")]
    public class MergeChainDatabase : AbstractDatabase<List<MergeChainVo>>
    {
        public override string AssociatedWorksheet => "MergeChains";

        public MergeChainVo GetChainByCategory(EMergeCategory category) => 
            data.GetBy(value => value.category == category);
        
        public MergeChainVo GetChainById(string id) => 
            data.GetBy(value => value.id == id);

        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseMergeChainTable(spreadSheet);
        }

    }
}
