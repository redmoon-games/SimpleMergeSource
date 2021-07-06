using Game.UI.Popups.BuildingGame;
using UnityEngine;

namespace Game.UI
{
    public class MapButton : MonoBehaviour
    {
        [SerializeField] private BuildingWindow buildingWindow;

        public void OnButtonClick()
        {
            buildingWindow.ShowWindow();
        }
    }
}
