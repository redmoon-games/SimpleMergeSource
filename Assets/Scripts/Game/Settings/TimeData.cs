using System;

namespace Game.Settings
{
    [Serializable]
    public struct TimeData
    {
        public int days;
        public int hours;
        public int minutes;
        public int seconds;

        public TimeData(int days = 1, int hours = 1, int minutes = 1, int seconds = 1)
        {
            this.days = days;
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public TimeSpan Value =>
            new TimeSpan(days, hours, minutes, seconds);
    }
}