using System.Collections.Generic;
using Core.Databases;
using Game.Databases.Helpers;
using Game.Models;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Game.Databases.Implementations
{
    [CreateAssetMenu(fileName = "RoomNodeDatabase", menuName = "Scriptable Database/RoomNode")]
    public class RoomNodeDatabase : AbstractDatabase<List<RoomNodeVo>>
    {
        public override string AssociatedWorksheet => "RoomNodes";
        
        public RoomNodeVo GetRoomById(string id) => 
            data.GetBy(value => value.id == id);
        
        public RoomNodeVo GetRoomByChain(string id) => 
            data.GetBy(value =>
            {
                var chain = value.mergeChains.GetBy(c => c == id);
                return chain != default;
            });

        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = DatabaseHelpers.ParseRoomNodeTable(ss);
        }
    }
}
