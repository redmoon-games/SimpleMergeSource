using Game.Controllers;
using Game.Models;
using Game.Signals.Base;

namespace Game.Signals
{
    public class WorldEndedSignal : SignalBase<WorldEndedData, WorldController>
    {
        public WorldEndedSignal(WorldEndedData value, WorldController owner) : base(value, owner)
        {
        }
    }

    public readonly struct WorldEndedData
    {
        public readonly MergeChainVo chainVo;
        public readonly WorldController controller;

        public WorldEndedData(MergeChainVo chainVo, WorldController controller)
        {
            this.chainVo = chainVo;
            this.controller = controller;
        }
    }
}