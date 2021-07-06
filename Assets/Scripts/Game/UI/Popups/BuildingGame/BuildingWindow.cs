using System;
using System.Collections.Generic;
using Game.Controllers;
using Game.Services;
using Game.Signals;
using Game.Signals.FromWindow;
using Game.UI.BottomBar;
using Plugins.WindowsManager;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.BuildingGame
{
    [Serializable]
    public class MetaGameData
    {
        public List<BuildingData> buildings = new List<BuildingData>();
        public List<BuildingTaskData> tasks = new List<BuildingTaskData>();
    }

    public class BuildingWindow : Window<BuildingWindow>
    {
        [SerializeField] private TaskListWindow taskListWindow;
        [SerializeField] private PaymentTypePopup paymentTypePopup;
        [SerializeField] private List<DeliveryTaskController> taskSlots;
        [SerializeField] private Transform buildingsRoot;
        [SerializeField] private BuildingController buildingPrefab;

        private readonly List<BuildingController> _buildings = new List<BuildingController>();

        private WalletService _walletService;
        private WindowManager _windowManager;
        private SignalBus _signalBus;
        private MetaGameData _data;
        private WorldData _worldData;
        private MetaGameService _metaGameService;

        [Inject]
        public void Construct(
            WalletService walletService,
            WindowManager windowManager,
            MetaGameService metaGameService,
            SignalBus signalBus
        )
        {
            _walletService = walletService;
            _windowManager = windowManager;
            _metaGameService = metaGameService;
            _signalBus = signalBus;
            _signalBus.Subscribe<MetaGameDataNotificationSignal>(OnDataChanged);
        }

        public void CloseWindow()
        {
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }

        public override void Activate(bool immediately = false)
        {
            taskListWindow.OnTaskChanged += OnTaskChanged;
            ActivatableState = ActivatableState.Active;
            gameObject.SetActive(true);
        }

        public override void Deactivate(bool immediately = false)
        {
            taskListWindow.OnTaskChanged -= OnTaskChanged;
            ActivatableState = ActivatableState.Inactive;
            gameObject.SetActive(false);
        }

        public void ShowWindow()
        {
            UpdateBuildings();
            UpdateTasks();

            if (ActivatableState != ActivatableState.Active)
            {
                _windowManager.CloseAll();
                _signalBus.Fire(new AllClosedSignal());

                _windowManager.ShowCreatedWindow(WindowId);
                _signalBus.Fire(new DownBarShowedSignal(EDownBarButtonType.Map));
            }
        }

        private void UpdateBuildings()
        {
            ResetBuildings();
            
            var buildings = _metaGameService.FilterTopLevelBuildings(_data.buildings);

            foreach (var buildingDto in buildings)
            {
                var building = Instantiate(buildingPrefab, buildingsRoot);
                building.Init(_metaGameService.GetBuildingInfo(buildingDto.BuildingVo.id));
                _buildings.Add(building);
            }
        }

        private void ResetBuildings()
        {
            foreach (var building in _buildings)
            {
                Destroy(building.gameObject);
            }
            
            _buildings.Clear();
        }

        private void UpdateTasks()
        {
            ResetSlots();
            var taskCount = 0;
            foreach (var task in _data.tasks)
            {
                var deliveryTime = _metaGameService.GetDeliveryTime(task.building.buildingId);
                taskSlots[taskCount].OnBuildingDelivered += OnBuildingDelivered;
                taskSlots[taskCount].OnSpeedupDelivery += OnSpeedupDelivery;
                taskSlots[taskCount++].Init(_data, task, deliveryTime);
            }
        }

        private void OnSpeedupDelivery(BuildingTaskData task)
        {
            paymentTypePopup.OnPaymentSuccess = OnDeliveryPayed;
            paymentTypePopup.ShowWindow(task, GetAvailablePayments(task));
        }

        private List<Payment> GetAvailablePayments(BuildingTaskData task)
        {
            var taskData = _metaGameService.GetTaskData(task.building.buildingId);
            var result = new List<Payment>
            {
                new Payment
                {
                    Amount = 30,
                    CommodityName = taskData.TaskName,
                    CommoditySprite = taskData.ItemSprite,
                    PaymentSprite = null,
                    Type = PaymentTypes.Video,
                    Enabled = true
                },
                new Payment
                {
                    Amount = taskData.DeliveryCost,
                    CommodityName = taskData.TaskName,
                    CommoditySprite = taskData.ItemSprite,
                    PaymentSprite = null,
                    Type = PaymentTypes.Crystal,
                    Enabled = _walletService.HasCrystalAmount(taskData.DeliveryCost)
                },
                new Payment
                {
                    Amount = 1,
                    CommodityName = taskData.TaskName,
                    CommoditySprite = taskData.ItemSprite,
                    PaymentSprite = taskData.UnitSprites?[0],
                    Type = PaymentTypes.Sacrifice,
                    Enabled = HasAllUnits(taskData.UnitIds),
                    UnitIds = taskData.UnitIds
                }
            };

            return result;
        }

        private bool HasAllUnits(IEnumerable<string> unitIds)
        {
            foreach (var unitId in unitIds)
            {
                if (!HasUnit(unitId)) return false;
            }

            return true;
        }

        private bool HasUnit(string unitId)
        {
            foreach (var room in _worldData.rooms)
            {
                foreach (var unit in room.units)
                {
                    if (unit.data.id == unitId) return true;
                }
            }

            return false;
        }

        private void OnDeliveryPayed(BuildingTaskData task, Payment payment)
        {
            ApplyPayment(payment);
            task.state = TaskState.Delivered;
            _signalBus.Fire(new MetaGameDataChangedSignal());
            UpdateTasks();
        }

        private void ApplyPayment(Payment payment)
        {
            switch (payment.Type)
            {
                case PaymentTypes.Crystal:
                    _walletService.TrySubtractCrystal(payment.Amount);
                    break;
                case PaymentTypes.Sacrifice:
                    foreach (var unitId in payment.UnitIds)
                    {
                        _signalBus.Fire(new UnitSacrificeSignal(unitId));
                    }
                    break;
            }
        }

        private void ResetSlots()
        {
            foreach (var taskSlot in taskSlots)
            {
                taskSlot.ResetTask();
                taskSlot.OnBuildingDelivered -= OnBuildingDelivered;
            }
        }

        public void ShowTaskList()
        {
            taskListWindow.ShowWindow(_data.tasks);
        }

        private void OnTaskChanged(TaskData taskData)
        {
            if (taskData.State == TaskState.Delivery)
                OnSpeedupDelivery(_data.tasks.FindById(taskData.BuildingId));
            else
                UpdateTasks();
        }

        private void OnDataChanged(MetaGameDataNotificationSignal signalData)
        {
            _worldData = signalData.data;
            _data = _worldData.metaGameData;
        }

        private void OnBuildingDelivered(BuildingTaskData taskData)
        {
            _data.tasks.Remove(taskData);
            _data.buildings.Add(taskData.building);
            
            UpdateTasks();
            UpdateBuildings();

            _signalBus.Fire(new MetaGameDataChangedSignal());
        }
    }
}