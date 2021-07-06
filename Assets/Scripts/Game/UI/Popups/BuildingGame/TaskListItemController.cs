using System;
using Game.Controllers;
using Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Game.UI.Popups.BuildingGame
{
    public class TaskListItemController : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI taskName;
        [SerializeField] private TextMeshProUGUI taskRequirementText;
        [SerializeField] private TextMeshProUGUI taskDescription;
        [SerializeField] private TextMeshProUGUI expAmount;
        [SerializeField] private TextMeshProUGUI timeAmount;
        [SerializeField] private TextMeshProUGUI goldAmount;
        [SerializeField] private TextMeshProUGUI crystalAmount;
        [SerializeField] private GameObject buyButton;
        [SerializeField] private GameObject deliverButton;
        [SerializeField] private GameObject bonusBuyInfo;
        [SerializeField] private GameObject deliveryTimer;
        [SerializeField] private TextMeshProUGUI deliveryTime;

        public Action<TaskData> OnItemAction;
        private readonly Timer _timer = new Timer();

        private TaskData _taskData;

        private WalletService _walletService;

        private void Update()
        {
            _timer.Update(Time.deltaTime);
        }

        public void Init(TaskData task, WalletService walletService)
        {
            _walletService = walletService;
            _taskData = task;
            taskName.text = task.TaskName;
            taskRequirementText.text = "";
            taskDescription.text = task.TaskDescription;
            itemImage.sprite = task.ItemSprite;
            expAmount.text = $"{task.Experience}";
            timeAmount.text = $"{task.TimeToDeliver}";
            goldAmount.text = $"{task.Cost}";
            crystalAmount.text = $"{task.DeliveryCost}";
            UpdateButtons();
        }

        public void OnButtonPressed()
        {
            switch (_taskData.State)
            {
                case TaskState.None:
                {
                    if (_walletService.TrySubtractGold(_taskData.Cost))
                        OnItemAction.Invoke(_taskData);
                    break;
                }
                case TaskState.Bought:
                case TaskState.Delivery:
                {
                    OnItemAction.Invoke(_taskData);
                    break;
                }
            }
        }

        private void UpdateButtons()
        {
            switch (_taskData.State)
            {
                case TaskState.None:
                    deliverButton.SetActive(false);
                    deliveryTimer.SetActive(false);
                    break;
                case TaskState.Bought:
                    buyButton.SetActive(false);
                    deliveryTimer.SetActive(false);
                    break;
                case TaskState.Delivery:
                    buyButton.SetActive(false);
                    bonusBuyInfo.SetActive(false);
                    OnTimerFinished();
                    _timer.onTimerFinished += OnTimerFinished;
                    break;
                case TaskState.Delivered:
                    buyButton.SetActive(false);
                    deliverButton.SetActive(false);
                    bonusBuyInfo.SetActive(false);
                    deliveryTimer.SetActive(false);
                    break;
            }
        }

        private void OnTimerFinished()
        {
            var currentTicks = DateTime.Now.Ticks;
            var remainedTicks = _taskData.TimeToDeliver * TimeUtils.TicksInSecond -
                                (currentTicks - _taskData.StartTime);

            if (remainedTicks >= 0)
            {
                deliveryTime.text = TimeUtils.TicksToHMS(remainedTicks);
                _timer.Reset(1);
            }
            else
            {
                deliveryTimer.SetActive(false);
                _timer.onTimerFinished -= OnTimerFinished;
                _timer.Reset();
            }
        }
    }
}