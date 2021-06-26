using System;
using Core.BigNumberAsset;
using Core.Databases;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI.Popups.UnitShop
{
    public enum UnitShopItemState
    {
        Unknown,
        Locked,
        BuyForCrystal,
        BuyForGold
    }
    
    public class UnitShopItemParams
    {
        public BigValue goldPrice;
        public int crystalPrice;
        public Sprite icon;
        public string name;
        public BigValue income;
        public UnitShopItemState state;
        public int level;
    }

    public class UnitShopItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI buyButtonText;
        [SerializeField] private Image buyButtonImage;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI unitName;
        [SerializeField] private TextMeshProUGUI income;
        [Space]
        [SerializeField] private Transform unitInfo;
        [SerializeField] private Transform lockedIcon;
        [SerializeField] private Transform unknownText;
        [Space]
        [SerializeField] private Sprite goldButtonTexture;
        [SerializeField] private Sprite crystalButtonTexture;
        [SerializeField] private Sprite lockedButtonTexture;
        [SerializeField] private Sprite unknownItemTexture;
        [Space]
        [SerializeField] private TMP_SpriteAsset goldSpriteAsset;
        [SerializeField] private TMP_SpriteAsset crystalSpriteAsset;

        public event Action<UnitShopItem> BoughtGold;
        public event Action<UnitShopItem> BoughtCrystal;

        public event Action<UnitShopItem> UnlockedForCrystal;

        private LocalizationDatabase _localizationDatabase;
        private UnitShopItemState _state;

        [Inject]
        public void Construct(LocalizationDatabase localizationDatabase)
        {
            _localizationDatabase = localizationDatabase;
        }

        public void Init(UnitShopItemParams p)
        {
            unitInfo.gameObject.SetActive(false);
            unknownText.gameObject.SetActive(false);

            switch (p.state)
            {
                case UnitShopItemState.Unknown:
                    unknownText.gameObject.SetActive(true);

                    SetButtonLocked();
                    icon.sprite = unknownItemTexture;
                    break;
                case UnitShopItemState.Locked:
                    SetButtonLocked();
                    SetUnitInfo(p);
                    break;
                case UnitShopItemState.BuyForCrystal:
                    SetButtonCrystal(p.crystalPrice);
                    SetUnitInfo(p);
                    break;
                case UnitShopItemState.BuyForGold:
                    SetButtonGold(p.goldPrice);
                    SetUnitInfo(p);
                    break;
            }

            _state = p.state;
        }

        private void SetUnitInfo(UnitShopItemParams p)
        {
            unitInfo.gameObject.SetActive(true);

            icon.sprite = p.icon;
            unitName.text = p.name;
            SetIncomeText(p.income);
        }

        public void OnBuyClick()
        {
            if(_state == UnitShopItemState.BuyForCrystal)
            {
                BoughtCrystal?.Invoke(this);
                return;
            }

            if (_state == UnitShopItemState.BuyForGold)
            {
                BoughtGold?.Invoke(this);
                return;
            }
        }

        private void SetIncomeText(BigValue incomeCount)
        {
            income.text =  $"Profit <sprite index=0> <color=#FFD925CC>{incomeCount.ToColorString()}/s";
        }

        private void SetButtonCrystal(int price)
        {
            lockedIcon.gameObject.SetActive(false);
            buyButtonImage.sprite = crystalButtonTexture;
            buyButtonText.spriteAsset = crystalSpriteAsset;

            buyButtonText.text = $"{price} <sprite index=0>";
            buyButtonText.gameObject.SetActive(true);
        }

        private void SetButtonGold(BigValue price)
        {
            lockedIcon.gameObject.SetActive(false);
            buyButtonImage.sprite = goldButtonTexture;
            buyButtonText.spriteAsset = goldSpriteAsset;

            buyButtonText.text = $"{price.ToString()} <sprite index=0>";
            buyButtonText.gameObject.SetActive(true);
        }

        private void SetButtonLocked()
        {
            lockedIcon.gameObject.SetActive(true);
            buyButtonText.gameObject.SetActive(false);
            buyButtonImage.sprite = lockedButtonTexture;
        }
    }
}
