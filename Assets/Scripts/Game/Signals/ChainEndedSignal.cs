using Game.Controllers;
using Game.Models;
using Game.Signals.Base;

namespace Game.Signals
{
    public class ChainEndedSignal : SignalBase<ChainEndedData, GridRoomController>
    {
        public ChainEndedSignal(ChainEndedData value, GridRoomController owner) : base(value, owner)
        {
        }
    }

    public readonly struct ChainEndedData
    {
        public readonly MergeChainVo chainVo;
        public readonly GridRoomController roomController;

        public ChainEndedData(MergeChainVo chainVo, GridRoomController roomController)
        {
            this.chainVo = chainVo;
            this.roomController = roomController;
        }
    }
}