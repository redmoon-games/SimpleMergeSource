using Game.Controllers;
using Game.Services;
using System;

namespace Core.SaveLoad
{
    [Serializable]
    public class SaveData
    {
        public RootData rootData;
        public WalletData walletData;
        public ListDictionary<string, ProgressUnit> progressData;

        public SaveData()
        {
            rootData = null;
            walletData = null;
            progressData = new ListDictionary<string, ProgressUnit>();
        }
    }
}
