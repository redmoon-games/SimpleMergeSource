using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Services
{
    [Serializable]
    public class ProgressUnit
    {
        public int goldBuyCount = 0;
        public int crystalBuyCount = 0;
        public bool isOpened = false;
    }
    
    public class ProgressManager : MonoBehaviour
    {
        public event Action DataChanged;
        public Dictionary<string, ProgressUnit> Data { get; private set; }

        public void Load(Dictionary<string, ProgressUnit> data)
        {
            data ??= new Dictionary<string, ProgressUnit>();

            Data = data;
        }

        public bool IsUnitOpened(string id) => Data.ContainsKey(id) && GetProgress(id).isOpened;

        public void OpenUnit(string id) 
        {
            var progressVo = GetProgress(id);

            progressVo.isOpened = true;
            DataChanged?.Invoke();
        }

        public void BuyUnit(string id ,bool isCrystal)
        {
            var progressVo = GetProgress(id);

            if (isCrystal)
            {
                progressVo.crystalBuyCount += 1;
            }
            else
            {
                progressVo.goldBuyCount += 1;
            }

            DataChanged?.Invoke();
        }

        public ProgressUnit GetProgress(string id) 
        {
            if (!Data.TryGetValue(id, out var progressVo))
            {
                progressVo = new ProgressUnit();
                Data.Add(id, progressVo);
            }

            return progressVo;
        }
    }
}
