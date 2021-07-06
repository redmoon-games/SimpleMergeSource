using Core.BigNumberAsset;
using Game.Databases.Implementations;
using Game.Services;
using Game.Signals;
using Game.UI;
using UnityEngine;
using Utils;
using Zenject;

namespace Game.Controllers
{
    public class IncomeController : MonoBehaviour
    {
        [SerializeField] private WorldController worldController;

        private Timer _timer;
        private BigValue _income = BigValue.Zero;
        
        private WalletService _walletService;
        private LevelDatabase _levelDatabase;
        private GameUI _gameUI;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(
            WalletService walletService,
            LevelDatabase levelDatabase,
            GameUI gameUI,
            SignalBus signalBus
        )
        {
            _walletService = walletService;
            _levelDatabase = levelDatabase;
            _gameUI = gameUI;
            _signalBus = signalBus;
            _signalBus.Subscribe<UnitDataChangedSignal>(OnDataChanged);
            _signalBus.Subscribe<RoomDataChangedSignal>(OnDataChanged);
            _signalBus.Subscribe<WorldDataChangedSignal>(OnDataChanged);
            _signalBus.Subscribe<UnitTappedSignal>(UnitTaped);
        }
        
        private void Start()
        {
            _timer = new Timer();
            _timer.onTimerFinished += OnTimerFinished;
            _timer.Init(0);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<UnitDataChangedSignal>(OnDataChanged);
            _signalBus.Unsubscribe<RoomDataChangedSignal>(OnDataChanged);
            _signalBus.Unsubscribe<WorldDataChangedSignal>(OnDataChanged);
            _signalBus.Unsubscribe<UnitTappedSignal>(UnitTaped);
        }
        
        private void Update()
        {
            _timer.Update(Time.deltaTime);
        }

        private void UnitTaped(UnitTappedSignal signal)
        {
            var levelVo = _levelDatabase.GetLevel(signal.Value.level + 1);
            _walletService.TryAddGold(levelVo.income, signal.Value.position);
        }

        public BigValue CalculateIncome(int times = 1)
        {
            return (BigValue) _income.Multiply(times);
        }
        
        private void OnTimerFinished()
        {
            var amount = CalculateIncome();
            _walletService.TryAddGold(amount);
            _timer.Reset(1);
        }


        private void OnDataChanged(UnitDataChangedSignal signal) => OnDataChanged(signal.IncomeChanged);
        private void OnDataChanged(RoomDataChangedSignal signal) => OnDataChanged(signal.IncomeChanged);
        private void OnDataChanged(WorldDataChangedSignal signal) => OnDataChanged(signal.IncomeChanged);
        
        private void OnDataChanged(bool incomeChanged)
        {
            if (incomeChanged)
            {
                _income = BigValue.Zero;
                foreach (var roomData in worldController.Rooms)
                {
                    foreach (var unitData in roomData.units)
                    {
                        if (unitData.state == ECharacterState.Idle)
                        {
                            var levelVo = _levelDatabase.GetLevel(unitData.level + 1);
                            _income.Add(levelVo.income);
                        }
                    }
                }
                _gameUI.UpdateIncome(_income);
            }
        }
    }
}
