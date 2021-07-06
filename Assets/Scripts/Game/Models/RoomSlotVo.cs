using System;
using UnityEngine;

namespace Game.Models
{
    [Serializable]
    public class RoomSlotVo
    {
        public string id;
        public string roomId;
        public string buildingTypeId;
        public float xCoord;
        public float yCoord;

        public Vector2 GetPosition() => new Vector2(xCoord, yCoord);
    }
}