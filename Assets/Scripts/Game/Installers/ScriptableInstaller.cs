using System.Collections.Generic;
using Core.Databases;
using Core.Databases.Audio;
using Game.Databases;
using Game.Databases.Implementations;
using UnityEngine;
using Zenject;

namespace Game.Installers
{
    [CreateAssetMenu(fileName = "ScriptableInstaller", menuName = "Installers/ScriptableInstaller")]
    public class ScriptableInstaller : ScriptableObjectInstaller<ScriptableInstaller>, ICollectable<SoundSource>
    {
        [SerializeField] private ResourceDatabase resourceDatabase;
        [SerializeField] private UnitDatabase unitDatabase;
        [SerializeField] private MergeChainDatabase mergeChainDatabase;
        [SerializeField] private MergeNodeDatabase mergeNodeDatabase;
        [SerializeField] private RoomNodeDatabase roomNodeDatabase;
        [SerializeField] private WorldChainDatabase worldChainDatabase;
        [SerializeField] private WorldNodeDatabase worldNodeDatabase;
        [SerializeField] private LocalizationDatabase localizationDatabase;
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private OfflinePopupLevelDatabase offlinePopupLevelDatabase;
        [SerializeField] private BuildingCostDatabase buildingCostDatabase;
        [SerializeField] private BuildingDatabase buildingDatabase;
        [SerializeField] private BuildingRequirementDatabase buildingRequirementDatabase;
        [SerializeField] private BuildingTypeDatabase buildingTypeDatabase;
        [SerializeField] private RoomSlotDatabase roomSlotDatabase;
        [SerializeField] private string folderToScan;
        [SerializeField] private List<SoundSource> soundSources = new List<SoundSource>();

        public override void InstallBindings()
        {
            Container.Bind<ResourceDatabase>().FromScriptableObject(resourceDatabase).AsSingle();
            Container.Bind<UnitDatabase>().FromScriptableObject(unitDatabase).AsSingle();
            Container.Bind<MergeChainDatabase>().FromScriptableObject(mergeChainDatabase).AsSingle();
            Container.Bind<MergeNodeDatabase>().FromScriptableObject(mergeNodeDatabase).AsSingle();
            Container.Bind<RoomNodeDatabase>().FromScriptableObject(roomNodeDatabase).AsSingle();
            Container.Bind<WorldChainDatabase>().FromScriptableObject(worldChainDatabase).AsSingle();
            Container.Bind<WorldNodeDatabase>().FromScriptableObject(worldNodeDatabase).AsSingle();
            Container.Bind<LocalizationDatabase>().FromScriptableObject(localizationDatabase).AsSingle();
            Container.Bind<LevelDatabase>().FromScriptableObject(levelDatabase).AsSingle();
            Container.Bind<OfflinePopupLevelDatabase>().FromScriptableObject(offlinePopupLevelDatabase).AsSingle();
            Container.Bind<BuildingCostDatabase>().FromScriptableObject(buildingCostDatabase).AsSingle();
            Container.Bind<BuildingDatabase>().FromScriptableObject(buildingDatabase).AsSingle();
            Container.Bind<BuildingRequirementDatabase>().FromScriptableObject(buildingRequirementDatabase).AsSingle();
            Container.Bind<BuildingTypeDatabase>().FromScriptableObject(buildingTypeDatabase).AsSingle();
            Container.Bind<RoomSlotDatabase>().FromScriptableObject(roomSlotDatabase).AsSingle();
            
            foreach (var soundSource in soundSources)
            {
                Container.QueueForInject(soundSource);
            }
        }

        public void SetData(List<SoundSource> data)
        {
            soundSources = data;
        }

        public string GetRootFolder()
        {
            return folderToScan;
        }
    }
}