using System;
using System.Collections.Generic;
using Core.Gestures;
using Game.Controllers;
using Game.UI.DeleteUnitIcon;
using UnityEngine;
using Zenject;

namespace Game.Services
{
    public class MoveGesture
    {
        public UnitController controller;
        public bool isMove = true;
    }

    public class MoveManager : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private SwipeTrailController swipeTrailController;
        [SerializeField] private DeleteUnit deleteUnit;

        private readonly Dictionary<int, MoveGesture> _activeInputs = new Dictionary<int, MoveGesture>();

        /// <summary>
        /// Event fired when a user performs a swipe gesture (not dragging some unit).
        /// </summary>
        public event Action<SwipeInput> Swipped;
        public event Action<UnitController> OnUnitTouched;
        public event Action<UnitController, Vector2> OnUnitMoved;
        public event Action<UnitController, Vector2> OnUnitReleased;

        private GestureController _gestureController;

        [Inject]
        public void Construct(
            GestureController gestureController
            )
        {
            _gestureController = gestureController;
        }

        private void OnEnable()
        {
            _gestureController.Pressed += OnGestureStart;
            _gestureController.DragFinished += OnGestureEnd;
            _gestureController.Dragged += OnDrag;
            _gestureController.Tapped += OnGestureEnd;
            _gestureController.Swiped += OnGestureEnd;
            _gestureController.DragStarted += OnGestureDragStart;

            OnUnitTouched += deleteUnit.Show;
            OnUnitMoved += deleteUnit.OnDragged;
            OnUnitReleased += deleteUnit.Hide;
        }

        private void OnDisable()
        {
            _gestureController.Pressed -= OnGestureStart;
            _gestureController.DragFinished -= OnGestureEnd;
            _gestureController.Dragged -= OnDrag;
            _gestureController.Tapped -= OnGestureEnd;
            _gestureController.Swiped -= OnGestureEnd;
            _gestureController.DragStarted -= OnGestureDragStart;
            
            OnUnitTouched -= deleteUnit.Show;
            OnUnitMoved -= deleteUnit.OnDragged;
            OnUnitReleased -= deleteUnit.Hide;
        }

        private void OnGestureStart(SwipeInput swipeInput)
        {
            Debug.Assert(!_activeInputs.ContainsKey(swipeInput.InputId));
            if (TryGetController(swipeInput.StartPosition, out var controller))
            {
                controller.Touch();

                if (controller.IsMovable)
                {
                    controller.UnitMoveInterrupted += OnMoveInterrupted;
                    _activeInputs.Add(swipeInput.InputId, new MoveGesture() { controller = controller });
                }
            }
            else
            {
                _activeInputs.Add(swipeInput.InputId, new MoveGesture() { isMove = false });
                swipeTrailController.SetActive(true, ScreenToWorldPoint(swipeInput.EndPosition));
            }
        }

        private void OnGestureDragStart(SwipeInput swipeInput)
        {
            if (!_activeInputs.TryGetValue(swipeInput.InputId, out var existingController))
            {
                return;
            }

            if (existingController.isMove)
            {
                existingController.controller.StartDrag();
                OnUnitTouched?.Invoke(existingController.controller);
            }
        }

        private void OnDrag(DragInput dragInput)
        {
            if (!_activeInputs.TryGetValue(dragInput.InputId, out var existingController))
            {
                // Probably caught by UI, or the input was otherwise lost
                return;
            }

            MoveController(dragInput);
        }

        private void OnGestureEnd(DragInput dragInput)
        {
            OnUnitReleased?.Invoke(null, dragInput.EndPosition);
            if (!_activeInputs.TryGetValue(dragInput.InputId, out var existingController))
            {
                // Probably caught by UI, or the input was otherwise lost
                return;
            }
            var moveGesture = _activeInputs[dragInput.InputId];
            _activeInputs.Remove(dragInput.InputId);

            if (moveGesture.isMove)
            {
                moveGesture.controller.FinishMove();
            }
            else
            {
                swipeTrailController.SetActive(false);
            }
        }

        private void OnGestureEnd(TapInput tapInput)
        {
            OnUnitReleased?.Invoke(null, tapInput.PressPosition);
            if (!_activeInputs.TryGetValue(tapInput.InputId, out var existingController))
            {
                if (TryGetController(tapInput.ReleasePosition, out var controller))
                {
                    controller.Tap();
                }
                return;
            }
            var moveGesture = _activeInputs[tapInput.InputId];
            _activeInputs.Remove(tapInput.InputId);

            if (moveGesture.isMove)
            {
                moveGesture.controller.Tap();
            }
            else
            {
                swipeTrailController.SetActive(false);
            }
        }

        private void OnGestureEnd(SwipeInput swipeInput)
        {
            OnUnitReleased?.Invoke(null, swipeInput.EndPosition);
            if (!_activeInputs.TryGetValue(swipeInput.InputId, out var existingController))
            {
                // Probably caught by UI, or the input was otherwise lost
                return;
            }
            var moveGesture = _activeInputs[swipeInput.InputId];
            _activeInputs.Remove(swipeInput.InputId);

            if (!moveGesture.isMove)
            {
                Swipped?.Invoke(swipeInput);
            }
            else
            {
                moveGesture.controller.FinishMove();
                swipeTrailController.SetActive(false);
            }
        }

        private bool TryGetController(Vector2 inputPosition, out UnitController unitController)
        {
            Vector2 position = mainCamera.ScreenToWorldPoint(inputPosition);
            RaycastHit2D hit = Physics2D.Raycast(position, Vector2.zero);
            if (hit)
            {
                var controller = hit.collider.GetComponent<UnitController>();
                if (controller != null)
                {
                    unitController = controller;
                    return true;
                }
            }

            unitController = null;
            return false;
        }

        private void MoveController(DragInput dragInput)
        {
            var moveGesture = _activeInputs[dragInput.InputId];
            var delta = ScreenToWorldPoint(dragInput.EndPosition) -
                ScreenToWorldPoint(dragInput.PreviousPosition);
            if (moveGesture.isMove)
            {
                moveGesture.controller.transform.position += delta;
                moveGesture.controller.Drag();
                OnUnitMoved?.Invoke(moveGesture.controller, dragInput.PreviousPosition);
            }
            else
            {
                swipeTrailController.transform.position += delta;
            }
        }

        private Vector3 ScreenToWorldPoint(Vector3 position)
        {
            var worldPosition = mainCamera.ScreenToWorldPoint(position);
            worldPosition.Scale(new Vector3(1, 1, 0));
            return worldPosition;
        }

        private bool TryGetInputIdByController(UnitController controller, out int InputId)
        {
            InputId = 0;

            foreach (var input in _activeInputs)
            {
                if (input.Value.controller == controller)
                {
                    InputId = input.Key;
                    return true;
                }
            }

            return false;
        }

        private void OnMoveInterrupted(UnitController unitController)
        {
            if (!TryGetInputIdByController(unitController, out var inputId))
            {
                return;
            }

            var moveGesture = _activeInputs[inputId];

            if (moveGesture.isMove)
            {
                _activeInputs.Remove(inputId);
                moveGesture.controller.FinishMove();
            }
        }
    }
}
