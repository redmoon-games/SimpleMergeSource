using System;
using System.Collections.Generic;
using Game.Controllers;
using Game.Models;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Utils;
using Object = UnityEngine.Object;

namespace Game.Databases
{
    [Serializable]
    public class PrefabResource<T> where T : MonoBehaviour
    {
        public T data;
        public string id;
    }
    
    [CreateAssetMenu(menuName = "Scriptable Database/ResourceDatabase", fileName = "ResourceDatabase")]
    public class ResourceDatabase : ScriptableObject
    {
        [SerializeField] private List<PrefabResource<GridRoomController>> roomPrefabs;
        [SerializeField] private List<PrefabResource<UnitController>> unitPrefabs;
        [SerializeField] private List<Image> unitImages;

        public GridRoomController GetRoomById(string roomId) => roomPrefabs
            .GetBy(value => value.id == roomId).data;
        public UnitController GetUnitById(string id) => unitPrefabs
            .GetBy(value => value.id == id).data;
        
        public Sprite GetUnitImageById(string id) => unitImages.GetBy(value => value.id == id).sprite;

        public void DownloadImages()
        {
#if UNITY_EDITOR
            DownloadToList(new[] {"Assets/Prefabs/Game/Rooms/Free"}, roomPrefabs);
            DownloadToList(new[] {"Assets/Prefabs/Game/Rooms/Grid"}, roomPrefabs, false);
            DownloadToList(new[] {"Assets/Prefabs/Game/Units"}, unitPrefabs);
            DownloadToList(new[] {"Assets/Sprites/Units"}, unitImages);
            Debug.Log($"[{nameof(ResourceDatabase)}] Update Success");
#endif
        }

        private void DownloadToList<T>(string[] folders, List<PrefabResource<T>> destination, bool clear = true) where T : MonoBehaviour
        {
#if UNITY_EDITOR
            if (clear)
                destination.Clear();
            
            var objects = new List<Object>();
            var guids = AssetDatabase.FindAssets($"t:GameObject", folders);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                objects.Add( AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
            }
            
            foreach (var obj in objects)
            {
                destination.Add(new PrefabResource<T>(){id = obj.name, data = ((GameObject) obj).GetComponent<T>()});
            }
#endif
        }

        private void DownloadToList(string[] folders, List<Image> destination)
        {
#if UNITY_EDITOR
            destination.Clear();
            var sprites = new List<Sprite>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", folders);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                sprites.Add((Sprite) AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)));
            }
            
            foreach (var sprite in sprites)
            {
                destination.Add(new Image(){id = sprite.name, sprite = sprite});
            }
#endif
        }
    }
}
