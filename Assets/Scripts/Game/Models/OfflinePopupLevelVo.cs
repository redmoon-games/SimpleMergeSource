using System;

namespace Game.Models
{
    [Serializable]
    public class OfflinePopupLevelVo
    {
        public int level;
        public SpinnerItemData[] spinerContent;
        public int crystalPrice;
        public bool canWatchAd;
    }
}
