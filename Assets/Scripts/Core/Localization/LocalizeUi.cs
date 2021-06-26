using Core.Databases;
using Game.Settings;
using TMPro;
using UnityEngine;
using Zenject;

namespace Core.Localization
{
    public class LocalizeUi : MonoBehaviour
    {
        private string fieldName;
        
        private LocalizationDatabase _localizationDatabase;
        private GameSettings _gameSettings;

        [Inject]
        public void Construct(
            GameSettings gameSettings,
            LocalizationDatabase localizationDatabase
        )
        {
            _gameSettings = gameSettings;
            _localizationDatabase = localizationDatabase;
        }

        private void Awake()
        {
            var text = GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                fieldName = text.text;
                text.text = _localizationDatabase.GetLocalizeText(fieldName, _gameSettings.language);
            }
        }
    }
}
