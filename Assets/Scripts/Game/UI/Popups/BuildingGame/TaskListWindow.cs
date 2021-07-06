using System;
using System.Collections.Generic;
using Core.BigNumberAsset;
using Game.Services;
using Game.Signals;
using Plugins.WindowsManager;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.BuildingGame
{
    public class TaskData
    {
        public string BuildingId;
        public string TaskName;
        public string TaskDescription;
        public List<string> UnitIds;
        public List<Sprite> UnitSprites;
        public BigValue Cost;
        public int DeliveryCost;
        public long Experience;
        public int TimeToDeliver;
        public long StartTime;
        public Sprite ItemSprite;
        public TaskState State;
    }

    public class TaskListWindow : Window<TaskListWindow>
    {
        [SerializeField] private Transform listRoot;
        [SerializeField] private TaskListItemController itemPrefab;
        
        public Action<TaskData> OnTaskChanged;

        private WalletService _walletService;
        private WindowManager _windowManager;
        private SignalBus _signalBus;
        private MetaGameService _metaGameService;
        private MetaGameData _data;

        private readonly List<TaskListItemController> _availableTasks = new List<TaskListItemController>();
        private Dictionary<string, BuildingTaskData> _currentTasks;

        [Inject]
        public void Construct(
            WalletService walletService,
            WindowManager windowManager,
            MetaGameService metaGameService,
            SignalBus signalBus)
        { 
            _windowManager = windowManager;
            _metaGameService = metaGameService;
            _signalBus = signalBus;
            _signalBus.Subscribe<MetaGameDataNotificationSignal>(OnDataChanged);
            _walletService = walletService;
        }
       
        public override void Activate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Active;
            gameObject.SetActive(true);
        }

        public override void Deactivate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Inactive;
            gameObject.SetActive(false);
        }
        
        public void ShowWindow(IEnumerable<BuildingTaskData> buildingTasks)
        {
            _currentTasks = buildingTasks.MapBuildingTaskData();
            UpdateTaskList();

            if (ActivatableState != ActivatableState.Active)
                _windowManager.ShowCreatedWindow(WindowId);
        }

        public void CloseWindow()
        {
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }

        private void OnTaskStateChanged(TaskData taskData)
        {
            if (taskData.State == TaskState.None)
            {
                _data.tasks.Add(new BuildingTaskData
                {
                    startTime = DateTime.Now.Ticks,
                    state = TaskState.Bought,
                    building = new BuildingData {buildingId = taskData.BuildingId}
                });
                
                _signalBus.Fire(new MetaGameDataChangedSignal());
            }
            else
            {
                taskData.State = TaskState.Delivery;
            }

            OnTaskChanged.Invoke(taskData);
            CloseWindow();
        }
        
        private void UpdateTaskList()
        {
            ClearTaskList();
            var tasks = _metaGameService.GetAvailableTasks(_data.buildings);

            foreach (var task in tasks)
            {
                if (_currentTasks.ContainsKey(task.BuildingId) && _currentTasks[task.BuildingId].state == TaskState.Delivered) continue;
               
                var listItem = Instantiate(itemPrefab, listRoot);
                task.State = GetTaskState(task.BuildingId);
                task.StartTime = GetStartTime(task.BuildingId);
                listItem.Init(task, _walletService);
                _availableTasks.Add(listItem);
                listItem.OnItemAction += OnTaskStateChanged;
            }
        }

        private long GetStartTime(string buildingId)
        {
            return _currentTasks.ContainsKey(buildingId) ? 
                _currentTasks[buildingId].startTime : 0;
        }

        private TaskState GetTaskState(string buildingId)
        {
            return _currentTasks.ContainsKey(buildingId) ? 
                _currentTasks[buildingId].state : 
                TaskState.None;
        }

        private void ClearTaskList()
        {
            foreach (var task in _availableTasks)
            {
                task.OnItemAction -= OnTaskStateChanged;
                Destroy(task.gameObject);
            }
            _availableTasks.Clear();
        }

        private void OnDataChanged(MetaGameDataNotificationSignal signalData)
        {
            _data = signalData.data.metaGameData;
        }
    }
}