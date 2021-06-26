using Game.Controllers;
using Game.Merge;
using Game.Signals.Base;

namespace Game.Signals
{
    public class MergeChainEndedSignal : SignalBase<MergedData, UnitController>
    {
        public MergeChainEndedSignal(MergedData value, UnitController owner) : base(value, owner) {}
    }
}
