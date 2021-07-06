using Core.BigNumberAsset;
using Core.SaveLoad;
using Game.Services;
using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Controllers
{
    public class CommandController : MonoBehaviour
    {
        [SerializeField] private WalletService wallet;
        [SerializeField] private RootController root;

        void Start()
        {
            DebugLogConsole.AddCommand("save.reset", "Reset save file ", ResetSaveCommand);

            DebugLogConsole.AddCommand<int>("crystal.add", "Add x crystals to wallet", AddCrystalCommand);
            DebugLogConsole.AddCommand<float>("gold.add", "Add x gold to wallet", AddGoldCommand);

            DebugLogConsole.AddCommand<int, int>("unit.spawn", "Spawns x units of level y", SpawnUnitsCommand);
        }

        private void ResetSaveCommand()
        {
            SaveDataService.ResetSaveData();
            Application.Quit();
        }

        private void AddCrystalCommand(int amount)
        {
            wallet.TryAddCrystal(amount);
        }

        private void AddGoldCommand(float amount)
        {
            wallet.TryAddGold(new BigValue(amount, 0));
        }
        private void SpawnUnitsCommand(int amount, int level)
        {
            for (int i = 0; i < amount; i++)
            {
                root.WorldController.SpawnUnit(level, false);
            }
        }
    }
}


