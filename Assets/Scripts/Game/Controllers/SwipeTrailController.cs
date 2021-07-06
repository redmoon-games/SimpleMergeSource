using System.Collections.Generic;
using UnityEngine;

namespace Game.Controllers
{
    public class SwipeTrailController : MonoBehaviour
    {
        private List<UnitController> _controllers = new List<UnitController>();
        
        private void Start()
        {
            SetActive(false);
        }

        public void SetActive(bool value, Vector3 position = default)
        {
            if (value)
            {
                transform.position = position;
            }
            gameObject.SetActive(value);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
        }

        private void OnCollisionExit2D(Collision2D other)
        {
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var unit = other.GetComponent<UnitController>();
            if (unit != null && !_controllers.Contains(unit))
            {
                _controllers.Add(unit);
                if (unit.IsMovable)
                {
                    unit.Tap();
                }
                else
                {
                    unit.Touch();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var unit = other.GetComponent<UnitController>();
            if (unit != null && _controllers.Contains(unit))
            {
                _controllers.Remove(unit);
            }
        }
    }
}
