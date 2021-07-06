using System;
using Core.BigNumberAsset;
using Plugins.WindowsManager;
using TMPro;
using UnityEngine;
using Zenject;
using UnityEngine.UI;
using DG.Tweening;
using Game.Settings;
using Game.Databases.Implementations;
using Game.Services;

namespace Game.UI.Popups.OfflineIncome
{
    public class OfflineIncome : Window<OfflineIncome>
    {
        [SerializeField] private TextMeshProUGUI offlineIncome;
        [SerializeField] private MultiplierSpinner spinner;
        [Space]
        [SerializeField] private Button buttonWatchAds;
        [SerializeField] private Button buttonCrystalBuy;
        [SerializeField] private Button buttonClose;
        [Space]
        [SerializeField] private TextMeshProUGUI textCrystalButton;

        public event Action Closed;
        public event Action<BigValue> TakenReward;

        private BigValue _reward;
        private Sequence _rewardSequence;
        private int _currentLevel;

        private Vector2 _crystalBuyButtonPosition;

        private WindowManager _windowManager;
        private OfflinePopupSettings _offlinePopupSettings;
        private OfflinePopupLevelDatabase _offlinePopupLevelDatabase;
        private WalletService _walletService;

        [Inject]
        public void Construct(
            WindowManager windowManager,
            OfflinePopupSettings offlinePopupSettings,
            OfflinePopupLevelDatabase offlinePopupLevelDatabase,
            WalletService walletService
            ) 
        {
            _windowManager = windowManager;
            _offlinePopupSettings = offlinePopupSettings;
            _offlinePopupLevelDatabase = offlinePopupLevelDatabase;
            _walletService = walletService;

            _crystalBuyButtonPosition = buttonCrystalBuy.transform.localPosition;
        }


        public override void Activate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Active;
            gameObject.SetActive(true);

            spinner.Rotated += OnSpinerRotated;
        }

        public override void Deactivate(bool immediately = false)
        {
            ActivatableState = ActivatableState.Inactive;
            gameObject.SetActive(false);

            spinner.Rotated -= OnSpinerRotated;
        }

        public void Init(BigValue income, int level) 
        {
            _currentLevel = level;

            var levelNode = _offlinePopupLevelDatabase.GetLevel(_currentLevel);

            spinner.Init(levelNode.spinerContent);
            SetCrystalPrice(levelNode.crystalPrice);
            SetWatchAdsButtonActive(levelNode.canWatchAd && IsAdAvailable());

            if (_rewardSequence == null)
            {
                _reward = income;
                offlineIncome.text = income.ToString();
            }
        }

        public void OnCloseButton()
        {
            TakenReward?.Invoke(_reward);
            OnClose();
        }

        public void OnWatchAdButton()
        {
            //TODO show ads

            RotateSpiner();
        }

        public void OnCrystalButton()
        {
            var levelNode = _offlinePopupLevelDatabase.GetLevel(_currentLevel);

            if (!_walletService.TrySubtractCrystal(levelNode.crystalPrice))
            {
                return;
            }

            RotateSpiner();
        }

        public void Show() => 
            _windowManager.ShowCreatedWindow(WindowId);

        public void OnClose()
        {
            Closed?.Invoke();

            if (_rewardSequence != null)
            {
                _rewardSequence.Kill(true);
            }

            Close();
        }

        private void RotateSpiner()
        {
            SetButtonsInteractable(false);
            spinner.Rotate();
        }

        private void OnSpinerRotated(float multiplier)
        {
            if (_rewardSequence != null)
            {
                _rewardSequence.Kill(true);
            }

            BigValue currentRewad = (BigValue) _reward.Clone();
            _reward.Multiply((decimal)multiplier);

            BigValue difference = (BigValue)_reward.Clone();
            difference.Subtract(currentRewad);

            float animationFillAmount = 0;

            _rewardSequence = DOTween.Sequence();
            _rewardSequence.Append(DOTween.To(() => animationFillAmount, x => animationFillAmount = x, 1f, _offlinePopupSettings.rewardAnimationTime).OnUpdate(() => 
            {
                BigValue step = (BigValue)difference.Clone();
                step.Multiply((decimal)animationFillAmount);

                BigValue currentRewadWithStep = (BigValue)currentRewad.Clone();
                currentRewadWithStep.Add(step);

                offlineIncome.text = currentRewadWithStep.ToString();
            })
            .OnComplete(() => OnRewardAnimationComplete(_reward))
            .OnKill(() => OnRewardAnimationComplete(_reward)));

            Init(_reward, GetNextLevel());
            SetButtonsInteractable(true);
        }

        private void OnRewardAnimationComplete(BigValue reward)
        {
            offlineIncome.text = reward.ToString();
            _rewardSequence = null;
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            buttonClose.interactable = isInteractable;
            buttonWatchAds.interactable = isInteractable;
            buttonCrystalBuy.interactable = isInteractable;
        }

        private int GetNextLevel()
        {
            if(_offlinePopupLevelDatabase.IsLastLevel(_currentLevel))
            {
                return _currentLevel;
            }

            return _currentLevel + 1;
        }

        private void SetCrystalPrice(int price)
        {
            textCrystalButton.text = $"<sprite index=0> {price}";
        }

        private void SetWatchAdsButtonActive(bool isActive)
        {
            buttonWatchAds.gameObject.SetActive(isActive);

            Vector2 crystalButtonPosition = new Vector2(0, _crystalBuyButtonPosition.y);
            if (isActive)
            {
                crystalButtonPosition = _crystalBuyButtonPosition;
            }

            buttonCrystalBuy.transform.localPosition = crystalButtonPosition;
        }

        private bool IsAdAvailable()
        {
            //TODO write a condition if ad viewing is available
            return true;
        }
    }
}
