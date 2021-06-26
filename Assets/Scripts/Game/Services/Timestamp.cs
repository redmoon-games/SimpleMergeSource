using System;
using Game.Controllers;
using Game.Signals;

namespace Game.Services
{
    public class Timestamp
    {
        private long _timestamp;

        public void Init(long timestamp) =>
            _timestamp = timestamp;

        public TimeSpan PlayerAbsenceTime() =>
            DateTime.UtcNow - LastSession();
        
        public long GetTicks() => DateTime.UtcNow.Ticks;

        private DateTime LastSession() =>
            _timestamp == 0 ?
                DateTime.Now :
                new DateTime(_timestamp);
    }
}
