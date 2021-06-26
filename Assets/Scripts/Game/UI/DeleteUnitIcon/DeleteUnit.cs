using System.Collections.Generic;
using Core.Gestures;
using Game.Controllers;
using Game.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.DeleteUnitIcon
{
    public class DeleteUnit : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private EventSystem eventSystem;
        [SerializeField] private GraphicRaycaster raycaster;

        [SerializeField] private string showBool = "Show";
        [SerializeField] private string highlightBool = "Highlight";
        
        public bool CanDelete { get; set; }

        private void Awake()
        {
            animator.SetBool(showBool, false);
        }

        public void OnDragged(UnitController unit, Vector2 position)
        {
            var results = new List<RaycastResult>();
            raycaster.Raycast(CreatePointerEventData(position), results);

            var deleteFound = FindDeleteUnit(results);
            CanDelete = deleteFound;
            animator.SetBool(highlightBool, deleteFound);
        }

        public void Show(UnitController unit)
        {
            animator.SetBool(showBool, true);
        }

        public void Hide(UnitController unit, Vector2 position)
        {
            animator.SetBool(showBool, false);
        }

        private bool FindDeleteUnit(List<RaycastResult> results)
        {
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.GetComponent<DeleteUnit>())
                {
                    return true;
                }
            }

            return false;
        }

        private PointerEventData CreatePointerEventData(Vector2 position)
        {
            var pointerEventData = new PointerEventData(eventSystem)
            {
                position = position
            };
            return pointerEventData;
        }
    }
}
