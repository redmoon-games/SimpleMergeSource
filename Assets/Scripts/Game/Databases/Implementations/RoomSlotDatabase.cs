using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "RoomSlotDatabase", menuName = "Scriptable Database/RoomSlots")]
    public class RoomSlotDatabase : AbstractDatabase<List<RoomSlotVo>>
    {
        public override string AssociatedWorksheet => "RoomSlots";
        
        public override void UpdateDatabase(GstuSpreadSheet spreadSheet)
        {
            data = DatabaseHelpers.ParseRoomSlotsTable(spreadSheet);
        }
        
        public RoomSlotVo GetByBuildingType(string buildingTypeId) =>
            data.GetBy(value => value.buildingTypeId == buildingTypeId);
    }
}