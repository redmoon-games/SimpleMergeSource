using System;
using Utils;

namespace Game.Models
{
    [Serializable]
    public class UnitVo
    {
        public string id;
        public string mergeNodeId;
        public FloatRange behaviourPauseRange;
    }
}
