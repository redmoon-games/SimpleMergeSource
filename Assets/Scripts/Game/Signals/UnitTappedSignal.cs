using Game.Controllers;
using Game.Signals.Base;
using UnityEngine;

namespace Game.Signals
{
    public class UnitTappedSignal : SignalBase<UnitTappedData, UnitController>
    {
        public UnitTappedSignal(UnitTappedData value, UnitController owner) : base(value, owner)
        {
        }
    }

    public readonly struct UnitTappedData
    {
        public readonly int level;
        public readonly Vector2 position;
        
        public UnitTappedData(int level, Vector2 position)
        {
            this.level = level;
            this.position = position;
        }
    }
}
