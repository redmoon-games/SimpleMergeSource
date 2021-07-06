using Game.Controllers;
using Game.Signals.Base;

namespace Game.Signals
{
    public class SpawnMaxReleasedSignal : SignalBase<SpawnMaxReleasedData, GridRoomController>
    {
        public SpawnMaxReleasedSignal(SpawnMaxReleasedData value, GridRoomController owner) : base(value, owner)
        {
        }
    }

    public readonly struct SpawnMaxReleasedData
    {
        public readonly int level;

        public SpawnMaxReleasedData(int level)
        {
            this.level = level;
        }
    }
}