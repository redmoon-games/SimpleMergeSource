using Game.Controllers;
using Game.Signals.Base;

namespace Game.Signals
{
    public enum EUnitSignalType
    {
        Moved,
        Dragged,
        StartDragged
    }

    public class UnitSignal : SignalBase<UnitSignalData, UnitController>
    {
        public UnitSignal(UnitSignalData value, UnitController owner) : base(value, owner)
        {
        }
    }

    public readonly struct UnitSignalData
    {
        public readonly EUnitSignalType type;
        public readonly UnitController unitRef;

        public UnitSignalData(EUnitSignalType type, UnitController unitRef)
        {
            this.type = type;
            this.unitRef = unitRef;
        }
    }
}
