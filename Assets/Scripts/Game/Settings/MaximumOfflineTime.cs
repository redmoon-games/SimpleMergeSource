using System;

namespace Game.Settings
{
    [Serializable]
    public class MaximumOfflineTime
    {
        public int days = 1;
        public int hours = 1;
        public int minutes = 1;
        public int seconds = 1;
        
        public TimeSpan Value =>
            new TimeSpan(days, hours, minutes, seconds);
    }
}
