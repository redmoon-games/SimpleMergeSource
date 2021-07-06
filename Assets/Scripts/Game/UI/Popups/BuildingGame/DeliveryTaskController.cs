using System;
using Game.Controllers;
using Game.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Zenject;

namespace Game.UI.Popups.BuildingGame
{
    public class DeliveryTaskController : MonoBehaviour
    {
        [SerializeField] private Sprite builtSprite;
        [SerializeField] private Sprite deliverySprite;
        [SerializeField] private Sprite deliveredSprite;
        [SerializeField] private GameObject timerUI;
        [SerializeField] private TextMeshProUGUI timeLeft;
        [SerializeField] private Image currentSprite;

        public Action<BuildingTaskData> OnBuildingDelivered;
        public Action<BuildingTaskData> OnSpeedupDelivery;

        private BuildingTaskData _data;
        private MetaGameData _metaGameData;
        private int _deliveryTime;
        private readonly Timer _timer = new Timer();
        private long RemainingTicks => _deliveryTime * TimeUtils.TicksInSecond - (DateTime.Now.Ticks - _data.startTime);

        private SignalBus _signalBus;

        [Inject]
        public void Construct(
            SignalBus signalBus)
        { 
            _signalBus = signalBus;
        }

        private void Update()
        {
            _timer.Update(Time.deltaTime);
        }

        public void ResetTask()
        {
            _data = null;
            timerUI.SetActive(false);
            timeLeft.text = "00:00:00";
            _timer.Reset();
            _timer.onTimerFinished -= OnTimerFinished;
            currentSprite.sprite = builtSprite;
            gameObject.SetActive(false);
        }

        public void Init(MetaGameData metaGameData, BuildingTaskData task, int deliveryTime)
        {
            _metaGameData = metaGameData;
            _data = task;
            _deliveryTime = deliveryTime;
            switch (_data.state)
            {
                case TaskState.Delivery when RemainingTicks <= 0:
                    _data.state = TaskState.Delivered;
                    timerUI.SetActive(false);
                    currentSprite.sprite = deliveredSprite;
                    break;
                case TaskState.Delivery:
                    InitDeliveryState();
                    break;
                case TaskState.Delivered:
                    timerUI.SetActive(false);
                    currentSprite.sprite = deliveredSprite;
                    break;
            }

            gameObject.SetActive(true);
        }

        private void InitDeliveryState()
        {
            currentSprite.sprite = deliverySprite;
            timerUI.SetActive(true);
            _timer.onTimerFinished += OnTimerFinished;
            _timer.Init(1);
            OnTimerFinished();
        }

        public void OnTaskClick()
        {
            if (_data == null) return;

            switch (_data.state)
            {
                case TaskState.Bought when _metaGameData.CountActiveDeliveries() != 0:
                    return;
                case TaskState.Bought:
                    StartDelivery();
                    break;
                case TaskState.Delivery:
                    OnSpeedupDelivery.Invoke(_data);
                    break;
                case TaskState.Delivered:
                    OnBuildingDelivered.Invoke(_data);
                    break;
            }
        }

        private void StartDelivery()
        {
            _data.state = TaskState.Delivery;
            _data.startTime = DateTime.Now.Ticks;

            timerUI.SetActive(true);
            _timer.onTimerFinished += OnTimerFinished;
            _timer.Init(1);
            OnTimerFinished();

            _signalBus.Fire(new MetaGameDataChangedSignal());
        }

        private void OnTimerFinished()
        {
            if (RemainingTicks >= 0)
            {
                timeLeft.text = TimeUtils.TicksToHMS(RemainingTicks);
                _timer.Reset(1);
            }
            else
            {
                timerUI.SetActive(false);
                _timer.Reset();
            }
        }
    }
}