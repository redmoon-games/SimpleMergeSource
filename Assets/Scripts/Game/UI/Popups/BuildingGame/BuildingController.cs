using System;
using Game.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups.BuildingGame
{
    [Serializable]
    public class BuildingTaskData
    {
        public BuildingData building;
        public long startTime;
        public TaskState state = TaskState.None;
    }

    [Serializable]
    public class BuildingData
    {
        public string buildingId;
    }

    public enum TaskState
    {
        None,
        Bought,
        Delivery,
        Delivered
    }
    public class BuildingController : MonoBehaviour
    {
        [SerializeField] private Image image;
        
        public void Init(BuildingDto building)
        {
            transform.localPosition = building.Position;
            image.sprite = building.Sprite;
            gameObject.SetActive(true);
        }
    }
}