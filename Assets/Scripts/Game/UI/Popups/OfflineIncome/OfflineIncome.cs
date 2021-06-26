using System;
using Core.BigNumberAsset;
using Plugins.WindowsManager;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.OfflineIncome
{
    public class OfflineIncome : Window<OfflineIncome>
    {
        [SerializeField] private TextMeshProUGUI offlineIncome;

        public Action Closed = delegate {};
        public event Action TakenReward;

        private WindowManager _windowManager;

        [Inject]
        public void Construct(WindowManager windowManager) =>
            _windowManager = windowManager;

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

        public void Init(BigValue income) => 
            offlineIncome.text = income.ToColorString();

        public void Take()
        {
            TakenReward?.Invoke();
            OnClose();
        }
        
        public void TakeX2()
        {
            TakenReward?.Invoke();
            TakenReward?.Invoke();
            OnClose();
        }

        public void Show() => 
            _windowManager.ShowCreatedWindow(WindowId);

        public void OnClose()
        {
            Closed.Invoke();
            Close();
        }
    }
}
