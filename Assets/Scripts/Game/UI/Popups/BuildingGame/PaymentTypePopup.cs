using System;
using System.Collections.Generic;
using Plugins.WindowsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI.Popups.BuildingGame
{
    public enum PaymentTypes
    {
        None,
        Crystal,
        Video,
        Sacrifice
    }

    public struct Payment
    {
        public PaymentTypes Type;
        public int Amount;
        public string CommodityName;
        public Sprite CommoditySprite;
        public Sprite PaymentSprite;
        public List<string> UnitIds;
        public bool Enabled;
    }

    public class PaymentTypePopup : Window<PaymentTypePopup>
    {
        [SerializeField] private GameObject payCrystalButton;
        [SerializeField] private GameObject payVideoButton;
        [SerializeField] private GameObject payUnitButton;
        [SerializeField] private TextMeshProUGUI crystalAmount;
        [SerializeField] private TextMeshProUGUI videoAmount;
        [SerializeField] private TextMeshProUGUI unitAmount;
        [SerializeField] private Image commoditySprite;
        [SerializeField] private Image unitSprite;

        public Action<BuildingTaskData, Payment> OnPaymentSuccess;
        private Dictionary<PaymentTypes, Action<Payment>> _initializers;

        private WindowManager _windowManager;
        private BuildingTaskData _data;
        private List<Payment> _payments;

        [Inject]
        public void Construct(
            WindowManager windowManager)
        {
            _windowManager = windowManager;
            InitInitializers();
        }

        private void InitInitializers()
        {
            _initializers = new Dictionary<PaymentTypes, Action<Payment>>
            {
                {PaymentTypes.Crystal, InitCrystalPayment},
                {PaymentTypes.Video, InitVideoPayment},
                {PaymentTypes.Sacrifice, InitUnitPayment}
            };
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

        public void ShowWindow(BuildingTaskData taskData, List<Payment> payments)
        {
            _data = taskData;
            _payments = payments;
            UpdateUI();
            if (ActivatableState != ActivatableState.Active)
                _windowManager.ShowCreatedWindow(WindowId);
        }

        private void UpdateUI()
        {
            HideButtons();
            commoditySprite.sprite = _payments[0].CommoditySprite;
            foreach (var payment in _payments)
            {
                _initializers[payment.Type].Invoke(payment);
            }
        }

        private void InitCrystalPayment(Payment payment)
        {
            payCrystalButton.SetActive(true);
            crystalAmount.text = $"{payment.Amount}";
            payCrystalButton.GetComponent<Button>().interactable = payment.Enabled;
        }

        private void InitVideoPayment(Payment payment)
        {
            payVideoButton.SetActive(true);
            videoAmount.text = $"{payment.Amount}";
            payVideoButton.GetComponent<Button>().interactable = payment.Enabled;
        }

        private void InitUnitPayment(Payment payment)
        {
            payUnitButton.SetActive(true);
            unitAmount.text = $"{payment.Amount}";
            unitSprite.sprite = payment.PaymentSprite;
            payUnitButton.GetComponent<Button>().interactable = payment.Enabled;
        }

        private void HideButtons()
        {
            payCrystalButton.SetActive(false);
            payVideoButton.SetActive(false);
            payUnitButton.SetActive(false);
        }

        public void CancelPayment()
        {
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }

        public void OnPayCrystals()
        {
            OnPaymentSuccess.Invoke(_data, GetPayment(PaymentTypes.Crystal));
           
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }

        private Payment GetPayment(PaymentTypes paymentType)
        {
            foreach (var payment in _payments)
            {
                if (payment.Type == paymentType) return payment;
            }

            throw new Exception($"Payment type {paymentType} not found");
        }

        public void OnPayVideo()
        {
            OnPaymentSuccess.Invoke(_data, GetPayment(PaymentTypes.Video));
           
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }

        public void OnPayUnit()
        {
            OnPaymentSuccess.Invoke(_data, GetPayment(PaymentTypes.Sacrifice));
           
            if (ActivatableState != ActivatableState.Inactive)
                _windowManager.CloseAll(GetType().Name);
        }
    }
}