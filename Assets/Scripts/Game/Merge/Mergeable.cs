using System;
using System.Linq;
using Game.Controllers;
using Game.Databases.Implementations;
using Game.Models;
using Game.Signals;
using UnityEngine;
using Zenject;

namespace Game.Merge
{
    public class Mergeable : MonoBehaviour
    {
        public event Action<MergedData> Merged = data => {};
        public event Action<MergedData> MergeChainEnded = data => {};
        
        public MergeNodeVo Data { get; set; }

        public bool IsMergeble { get; set; }

        private MergeChainDatabase _mergeChainDatabase;

        [Inject]
        public void Construct(
            MergeChainDatabase mergeChainDatabase
        )
        {
            _mergeChainDatabase = mergeChainDatabase;
        }

        public void Init(MergeNodeVo data)
        {
            Data = data;
        }

        public bool CanMerge(Mergeable mergeable)
        {
            var chain = _mergeChainDatabase.GetChainByCategory(Data.category);
            if(
                mergeable != null && 
                mergeable != this && 
                mergeable.IsMergeble && 
                IsMergeble &&
                !chain.Last(Data.id))
            {
                return Data.type == mergeable.Data.type;
            }

            return false;
        }

        public void Merge(Mergeable mergeable)
        {
            if (CanMerge(mergeable))
            {
                var chain = _mergeChainDatabase.GetChainByCategory(Data.category);
                var data = new MergedData(this, mergeable, chain);
                if (chain.Last(Data.id))
                {
                    MergeChainEnded.Invoke(data);
                }
                else
                {
                    Merged.Invoke(data);
                }
            }
        }
    }
}