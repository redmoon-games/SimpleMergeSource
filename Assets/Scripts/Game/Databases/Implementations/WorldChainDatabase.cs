using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "WorldChainDatabase", menuName = "Scriptable Database/WorldChain")]
    public class WorldChainDatabase : AbstractDatabase<WorldChainVo>
    {
        public override string AssociatedWorksheet => "WorldChain";
        
        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseWorldChainTable(spreadSheet);
        }
    }
}
