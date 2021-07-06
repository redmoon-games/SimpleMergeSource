using System;
using Core.BigNumberAsset;
using Game.Databases.Implementations;
using Game.Services;
using Game.Settings;
using Game.UI;
using Game.Signals;
using Game.UI.Popups.OfflineIncome;
using Game.UI.Popups.UnitUnlock;
using UnityEngine;
using Zenject;
using Game.UI.Popups.UnitShop;

namespace Game.Controllers
{
    [Serializable]
    public class RootData
    {
        public WorldData currentWorld;
    }

    public class RootController : MonoBehaviour
    {
        [SerializeField] private WalletService walletService;
        [SerializeField] private WorldController prefabWorld;
        [SerializeField] private UnitUnlockPanel unitUnlockPanel;
        [SerializeField] private OfflineIncome offlineIncome;
        [SerializeField] private UnitShop unitShop;
        [SerializeField] private string worldNodeId = string.Empty;

        public WorldController WorldController { get; private set; }
        public IncomeController IncomeController { get; private set; }

        public RootData Data { get; private set; }
        private readonly Timestamp _timestamp = new Timestamp();

        private WorldChainDatabase _worldChainDatabase;
        private WorldNodeDatabase _worldNodeDatabase;
        private GameSettings _gameSettings;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(
            WorldChainDatabase worldChainDatabase,
            WorldNodeDatabase worldNodeDatabase,
            GameSettings gameSettings,
            SignalBus signalBus
        )
        {
            _gameSettings = gameSettings;
            _worldNodeDatabase = worldNodeDatabase;
            _worldChainDatabase = worldChainDatabase;
            _signalBus = signalBus;
        }

        public void Load(RootData data)
        {
            if (data == null)
            {
                data = new RootData();
                if (worldNodeId == string.Empty)
                {
                    var first = _worldChainDatabase.GetData().First();
                    data.currentWorld = new WorldData {node = _worldNodeDatabase.GetNodeById(first)};
                }
                else
                {
                    data.currentWorld = new WorldData {node = _worldNodeDatabase.GetNodeById(worldNodeId)};
                }
            }

            Data = data;

            Init();
        }

        public void InterruptMove()
        {
            if (WorldController != null)
            {
                WorldController.InterruptMove();
            }
        }

        public void UpgradeGrid()
        {
            WorldController.UpgradeGrid();
        }

        private void Init()
        {
            var world = Instantiate(prefabWorld, transform);
            WorldController = world;
            IncomeController = world.GetComponent<IncomeController>();
            world.Init(Data.currentWorld);

            _signalBus.Subscribe<UnitOpenedSignal>(OnUnitOpened);
            unitShop.Bought += OnBought;

            OpenOfflineIncomeWindow();
        }

        private void OnUnitOpened(UnitOpenedSignal signal)
        {
            unitUnlockPanel.Init(signal.OpenedUnit, signal.Units);
            unitUnlockPanel.Show();
        }

        private void OnBought(int level) => WorldController.SpawnUnit(level);

        private void OpenOfflineIncomeWindow()
        {
            _timestamp.Init(Data.currentWorld.savedAt);
            OnOfflineIncomeClosed();
            
            if (NotMuchTimeHasPassed())
            {
                return;
            }

            BigValue incomePerSecond = IncomeController.CalculateIncome();
            BigValue baseReward = OfflineReward(incomePerSecond);

            offlineIncome.TakenReward += (BigValue reward) => walletService.TryAddGold(reward);
            offlineIncome.Closed += () => WorldController.ProgressManager.OnDataChanged();

            offlineIncome.Init((BigValue) baseReward.Clone(), 1);
            offlineIncome.Show();

            Data.currentWorld.savedAt = DateTime.UtcNow.Ticks;
        }

        private void OnOfflineIncomeClosed()
        {
            _signalBus.Subscribe<UnitDataChangedSignal>(_ => WorldController.UpdateSavedTime(_timestamp.GetTicks()));
            _signalBus.Subscribe<RoomDataChangedSignal>(_ => WorldController.UpdateSavedTime(_timestamp.GetTicks()));
            _signalBus.Subscribe<WorldDataChangedSignal>(_ => WorldController.UpdateSavedTime(_timestamp.GetTicks()));
        }

        private BigValue OfflineReward(BigValue incomePerSecond) =>
            _timestamp.PlayerAbsenceTime() > _gameSettings.offlineTimeIncome.max.Value ?
                CalculateReward(_gameSettings.offlineTimeIncome.max.Value, incomePerSecond) :
                CalculateReward(_timestamp.PlayerAbsenceTime(), incomePerSecond);

        private BigValue CalculateReward(TimeSpan playerAbsenceTime, BigValue incomePerSecond) =>
            (BigValue) incomePerSecond.Multiply((decimal) playerAbsenceTime.TotalSeconds);

        private bool NotMuchTimeHasPassed() =>
            _timestamp.PlayerAbsenceTime() < _gameSettings.offlineTimeIncome.min.Value;
    }
}