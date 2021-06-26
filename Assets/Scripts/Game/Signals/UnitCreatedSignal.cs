using Game.Controllers;
using Game.Models;
using Game.Signals.Base;

namespace Game.Signals
{
    public class UnitCreatedSignal : SignalBase<UnitCreatedData, GridRoomController>
    {
        public UnitCreatedSignal(UnitCreatedData value, GridRoomController owner) : base(value, owner)
        {
        }
    }

    public readonly struct UnitCreatedData
    {
        public readonly UnitData data;
        public readonly MergeChainVo chainVo;

        public UnitCreatedData(UnitData data, MergeChainVo chainVo)
        {
            this.data = data;
            this.chainVo = chainVo;
        }
    }
}
