using System.Collections.Generic;
using Game.Controllers;
using Game.Models;

namespace Game.Signals
{
    public readonly struct UnitOpenedSignal
    {
        public UnitVo OpenedUnit { get; }
        public List<UnitVo> Units { get; }
        
        public UnitOpenedSignal(UnitVo openedUnit, List<UnitVo> units)
        {
            OpenedUnit = openedUnit;
            Units = units;
        }
    }
}
