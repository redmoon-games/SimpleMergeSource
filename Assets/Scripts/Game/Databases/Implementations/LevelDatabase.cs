using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Scriptable Database/Level")]
    public class LevelDatabase : AbstractDatabase<List<LevelVo>>
    {
        public override string AssociatedWorksheet => "Levels";
        
        public LevelVo GetLevel(int level) => 
            data.GetBy(value => value.level == level);

        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = DatabaseHelpers.ParseLevelTable(ss);
        }
    }
}
