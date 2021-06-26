using System;
using System.Collections.Generic;
using Core.BigNumberAsset;
using Core.Pool;
using Game.Controllers;
using Game.Databases;
using Game.Databases.Implementations;
using Game.Models;
using Game.Services;
using Game.Settings;
using Game.Signals;
using Plugins.WindowsManager;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.UnitShop
{
    public class UnitShopParams
    {
        public int startLevel;
        public int maxUnlockedLevel;
        public int maxLevel;
        public MergeChainVo chain;
    }
    
    public class UnitShop : Window<UnitShop>
    {
        [SerializeField] private UnitShopItem prefabItem;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private RootController rootController;

        private Pool<UnitShopItem> _itemPool;
        private Dictionary<UnitShopItem, UnitShopItemParams> _itemParams = new Dictionary<UnitShopItem, UnitShopItemParams>();
        private UnitShopParams _currentShopParams;

        private ResourceDatabase _resourceDatabase;
        private ProgressManager _progressManager;
        private LevelDatabase _levelDatabase;
        private GameSettings _gameSettings;
        private WindowManager _windowManager;
        private WalletService _walletService;
        private SignalBus _signalBus;

        public event Action<int> Bought;

        [Inject]
        public void Construct(
            ResourceDatabase resourceDatabase,
            ProgressManager progressManager,
            LevelDatabase levelDatabase,
            GameSettings gameSettings,
            WindowManager windowManager,
            WalletService walletService,
            SignalBus signalBus
        )
        {
            _resourceDatabase = resourceDatabase;
            _progressManager = progressManager;
            _levelDatabase = levelDatabase;
            _gameSettings = gameSettings;
            _windowManager = windowManager;
            _walletService = walletService;
            _signalBus = signalBus;
            
            _itemPool = new Pool<UnitShopItem>(
                () =>
                {
                    var item = Instantiate(prefabItem, itemRoot);
                    item.gameObject.SetActive(false);
                    return item;
                },
                8
            );

            _signalBus.Subscribe<RoomChangedSignal>(OnRoomChanged);
            _signalBus.Subscribe<UnitOpenedSignal>(OnUnitOpened); 
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
        
        public void Show()
        {
            if(_windowManager.GetWindow(WindowId) == null)
            {
                _windowManager.ShowCreatedWindow(WindowId);
            }
        }
        
        public void Init(UnitShopParams p)
        {
            _itemParams.Clear();
            foreach (var unitShopItem in _itemPool.GetActive())
            {
                Recycle(unitShopItem);
            }

            var level = p.startLevel + 1;
            for (int i = 0; i < p.chain.nodes.Count; i++)
            {
                var node = p.chain.nodes[i];
                var levelVo = _levelDatabase.GetLevel(level);
                var progressVo = _progressManager.GetProgress(node);

                var itemParams = new UnitShopItemParams()
                {
                    goldPrice = levelVo.GetGoldPrice(progressVo.goldBuyCount),
                    crystalPrice = levelVo.GetCrystalPrice,
                    icon = _resourceDatabase.GetUnitImageById(node),
                    name = node,
                    income = levelVo.income,
                    level = level - 1,
                    state = GetUnitShopItemState(level - 1, p.maxUnlockedLevel, p.startLevel)
                };
                var item = _itemPool.Take();
                _itemParams.Add(item, itemParams);

                item.Init(itemParams);
                item.BoughtCrystal += OnBoughtCrystal;
                item.BoughtGold += OnBoughtGold;

                item.transform.SetSiblingIndex(i);
                item.gameObject.SetActive(true);
                level++;
            }
        }

        public void OnCloseClick()
        {
            if (_windowManager.GetWindow(WindowId) != null)
            {
                Close();
            }
        }

        private void Recycle(UnitShopItem item)
        {
            item.BoughtCrystal -= OnBoughtCrystal;
            item.BoughtGold -= OnBoughtGold;
            item.gameObject.SetActive(false);
            _itemPool.Recycle(item);
        }

        private void OnRoomChanged(RoomChangedSignal signal)
        {
            var controller = signal.Controller;
            var shopParams = new UnitShopParams()
            {
                chain = controller.Chain,
                startLevel = controller.StartLevel,
                maxLevel = controller.MaxLevel,
                maxUnlockedLevel = GetMaxUnlockedLevel(controller.Chain, controller.StartLevel, controller.MaxLevel)
            };

            _currentShopParams = shopParams;
            Init(shopParams);
        }

        private void OnBoughtGold(UnitShopItem item)
        {
            var itemParams = _itemParams[item];

            if (!_walletService.TrySubtractGold(itemParams.goldPrice))
            {
                return;
            }

            _progressManager.BuyUnit(itemParams.name, false);
            OnBought(item, itemParams);
        }

        private void OnBoughtCrystal(UnitShopItem item)
        {
            var itemParams = _itemParams[item];

            if (!_walletService.TrySubtractCrystal(itemParams.crystalPrice))
            {
                return;
            }
            _progressManager.BuyUnit(itemParams.name, true);
            OnBought(item, itemParams);
        }

        private void OnBought(UnitShopItem item, UnitShopItemParams itemParams)
        {
            var progressVo = _progressManager.GetProgress(itemParams.name);
            var levelVo = _levelDatabase.GetLevel(itemParams.level + 1);

            itemParams.goldPrice = levelVo.GetGoldPrice(progressVo.goldBuyCount);
            item.Init(itemParams);

            Bought?.Invoke(itemParams.level);
        }

        private void OnUnitOpened(UnitOpenedSignal signal)
        {
            if(_currentShopParams != null)
            {
                int maxUnlockedLevel = GetMaxUnlockedLevel(_currentShopParams.chain, _currentShopParams.startLevel, _currentShopParams.maxLevel);

                foreach (var item in _itemParams)
                {
                    item.Value.state = GetUnitShopItemState(item.Value.level, maxUnlockedLevel, _currentShopParams.startLevel);
                    item.Key.Init(item.Value);
                }
            }
        }

        private int GetMaxUnlockedLevel(MergeChainVo chain, int startLevel, int maxLevel)
        {
            for (int i = maxLevel - 1; i >= startLevel; i--)
            {
                if (_progressManager.IsUnitOpened(chain.nodes[i - startLevel]))
                {
                    return i + 1;
                }
            }

            return startLevel;
        }

        private UnitShopItemState GetUnitShopItemState(int level, int maxUnlockedLevel, int startLevel)
        {
            int roomLevel =  (level + 1) - startLevel;

            if (level >= maxUnlockedLevel)
            {
                return UnitShopItemState.Unknown;
            }

            if (level >= maxUnlockedLevel - _gameSettings.unitShopUnlockShift)
            {
                return UnitShopItemState.Locked;
            }

            if (level >= maxUnlockedLevel - (_gameSettings.unitShopUnlockShift + _gameSettings.unitShopCrystalBuyShift) && roomLevel > _gameSettings.unitShopOnlyGoldCount)
            {
                return UnitShopItemState.BuyForCrystal;
            }

            return UnitShopItemState.BuyForGold;
        }
    }
}

