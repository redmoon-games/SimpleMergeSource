using System;
using UnityEngine;

namespace Game.Settings
{
    [Serializable]
    public class GameSettings
    {
        [Header("Spawn Settings")]
        public SpawnSettings spawnSettings;
        
        [Header("Room Settings")]
        public RoomSettings roomSettings;

        [Header("Income Settings")]
        public IncomeSettings incomeSettings;
        
        [Header("Swipe Settings")]
        public SwipeSettings swipeSettings;
        
        [Header("Maximum offline time Settings")]
        public MaximumOfflineTime maximumOfflineTime;

        [Header("Localization")]
        public string language;
        
        [Header("UnitShop")]
        public int unitShopUnlockShift = 1;
        public int unitShopCrystalBuyShift = 2;
        public int unitShopOnlyGoldCount = 3;
    }
}
