using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "OfflinePopupLevelDatabase", menuName = "Scriptable Database/OfflinePopupLevel")]
    public class OfflinePopupLevelDatabase : AbstractDatabase<List<OfflinePopupLevelVo>>
    {
        public override string AssociatedWorksheet => "OfflinePopupLevels";

        public OfflinePopupLevelVo GetLevel(int level) => data.GetBy(value => value.level == level);

        public bool IsLastLevel(int level) => data[data.Count - 1].level == level;

        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = DatabaseHelpers.ParseOfflinePopupLevelTable(ss);
        }
    }
}
