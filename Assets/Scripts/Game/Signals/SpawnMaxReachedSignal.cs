using Game.Controllers;
using Game.Signals.Base;

namespace Game.Signals
{
    public class SpawnMaxReachedSignal : SignalBase<SpawnMaxReachedData, GridRoomController>
    {
        public SpawnMaxReachedSignal(SpawnMaxReachedData value, GridRoomController owner) : base(value, owner)
        {
        }
    }

    public readonly struct SpawnMaxReachedData
    {
        public readonly int level;

        public SpawnMaxReachedData(int level)
        {
            this.level = level;
        }
    }
}