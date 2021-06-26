using Game.Controllers;
using Game.Merge;
using Game.Signals.Base;

namespace Game.Signals
{
    public class MergedSignal : SignalBase<MergedData, UnitController>
    {
        public MergedSignal(MergedData value, UnitController owner) : base(value, owner) {}
    }
}
