using System;
using System.Collections.Generic;

namespace Game.Models
{
    [Serializable]
    public class WorldChainVo
    {
        public string id;
        public List<string> nodes;

        public string First() => nodes[0];
        public bool IsLast(string nodeVo) => nodeVo == nodes[nodes.Count - 1];
    }
}
