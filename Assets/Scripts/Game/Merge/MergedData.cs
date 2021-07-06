using Game.Models;

namespace Game.Merge
{
    public readonly struct MergedData
    {
        public readonly Mergeable source;
        public readonly Mergeable destination;
        public readonly MergeChainVo chain;
        
        public MergedData(Mergeable source, Mergeable destination, MergeChainVo chain)
        {
            this.source = source;
            this.destination = destination;
            this.chain = chain;
        }
        
        public int NextNode()
        {
            for (var i = 0; i < chain.nodes.Count - 1; i++)
            {
                var chainNode = chain.nodes[i];
                if (chainNode == source.Data.id)
                {
                    return i + 1;
                }
            }

            return -1;
        }
    }
}
