using Game.Controllers;
using Game.Services;
using System.IO;
using System.Text;
using Game.Signals;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Core.SaveLoad
{
    public class SaveDataService : MonoBehaviour
    {
        private static string _fileName = "saves";

        [SerializeField] private RootController rootController;
        [SerializeField] private WalletService walletService;
        [SerializeField] private ProgressManager progressManager;

        private bool _needSave;
         
        private SignalBus _signalBus;
        
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            Load();

            SubscribeEvents();
        }

        private void Update()
        {
            if (_needSave)
            {
                _needSave = false;
                Save();
            }
        }

        private void SubscribeEvents()
        {
            progressManager.DataChanged += NeedSave;
            _signalBus.Subscribe<UnitDataChangedSignal>(NeedSave);
            _signalBus.Subscribe<RoomDataChangedSignal>(NeedSave);
            _signalBus.Subscribe<WorldDataChangedSignal>(NeedSave);
            _signalBus.Subscribe<MetaGameDataChangedSignal>(NeedSave);
        }

        private void NeedSave() => _needSave = true;

        private void Save()
        {
            var filePath = GetLocalPath(_fileName);
            var data = new SaveData
            {
                rootData = rootController.Data,
                walletData = walletService.GetData,
                progressData = new ListDictionary<string, ProgressUnit>(progressManager.Data)
            };

            var json = JsonUtility.ToJson(data, true);
            
            using var sw = new StreamWriter(filePath, false, Encoding.Default);
            sw.WriteLine(json);
        }

        private void Load()
        {
            var filePath = GetLocalPath(_fileName);

            var data = new SaveData();

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var loadedData = JsonUtility.FromJson<SaveData>(json);

                if (loadedData != null)
                {
                    data = loadedData;
                }
            }

            walletService.Load(data.walletData);
            progressManager.Load(data.progressData.ToDictionary());
            rootController.Load(data.rootData);
        }

        private static string GetLocalPath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName + ".json");
        }
#if UNITY_EDITOR
        [MenuItem("Tools/Reset save data")]
#endif
        public static void ResetSaveData()
        {
            var filePath = GetLocalPath(_fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
