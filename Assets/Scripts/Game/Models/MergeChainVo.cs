using System;
using System.Collections.Generic;
using Game.Enums;

namespace Game.Models
{
    [Serializable]
    public class MergeChainVo
    {
        public EMergeCategory category;
        public string id;
        public List<string> nodes;
        public string startFrom;
        public List<string> endWith;

        public bool Last(string nodeVo) => nodeVo == nodes[nodes.Count - 1];
        public string Get(int index) => nodes[index];
    }
}
