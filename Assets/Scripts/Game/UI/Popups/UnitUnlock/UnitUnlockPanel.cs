using System.Collections.Generic;
using Core.Databases;
using Game.Databases;
using Game.Models;
using Game.Settings;
using Plugins.WindowsManager;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.UnitUnlock
{
    public class UnitUnlockPanel : Window<UnitUnlockPanel>
    {
        [SerializeField] private UnitUnlockChain chain;
        [SerializeField] private UnitUnlockAvatar unitAvatar;
        
        private ResourceDatabase _resourceDatabase;
        private LocalizationDatabase _localizationDatabase;
        private GameSettings _gameSettings;
        private WindowManager _windowManager;

        [Inject]
        public void Construct(
            ResourceDatabase resourceDatabase,
            LocalizationDatabase localizationDatabase,
            GameSettings gameSettings,
            WindowManager windowManager
        )
        {
            _resourceDatabase = resourceDatabase;
            _localizationDatabase = localizationDatabase;
            _gameSettings = gameSettings;
            _windowManager = windowManager;
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
            _windowManager.ShowCreatedWindow(WindowId);
        }
        
        public void Init(UnitVo unitToUnlock, List<UnitVo> units)
        {
            var nameKey = unitToUnlock.id + "Name";
            var unitName = nameKey;//_localizationDatabase.GetLocalizeText(nameKey, _gameSettings.language);
            var image = _resourceDatabase.GetUnitImageById(unitToUnlock.id);
            unitAvatar.Init(unitName, image);
            chain.Init(unitToUnlock, units);
        }

        public void OnClick()
        {
            Close();
        }
    }
}