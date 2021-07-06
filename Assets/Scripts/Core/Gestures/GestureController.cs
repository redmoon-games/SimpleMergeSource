using System;
using System.Collections.Generic;
using System.Text;
using Core.Drawing;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gestures
{
    /// <summary>
    /// Controller that interprets takes pointer input from <see cref="PointerInputManager"/> and detects
    /// directional swipes and detects taps.
    /// </summary>
    public class GestureController : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.Camera camera;

        [SerializeField]
        private PointerInputManager inputManager;

        // Maximum duration of a press before it can no longer be considered a tap.
        [SerializeField]
        private float maxTapDuration = 0.2f;

        // Maximum distance in screen units that a tap can drift from its original position before
        // it is no longer considered a tap.
        [SerializeField]
        private float maxTapDrift = 5.0f;

        // Maximum duration of a swipe before it is no longer considered to be a valid swipe.
        [SerializeField]
        private float maxSwipeDuration = 0.5f;

        // Minimum distance in screen units that a swipe must move before it is considered a swipe.
        // Note that if this is smaller or equal to maxTapDrift, then it is possible for a user action to be
        // returned as both a swipe and a tap.
        [SerializeField]
        private float minSwipeDistance = 10.0f;

        // How much a swipe should consistently be in the same direction before it is considered a swipe.
        [SerializeField]
        private float swipeDirectionSamenessThreshold = 0.6f;

        [SerializeField]
        private float updateDelay = 0.08f;

        private float _updateDelayTimer;

        [Header("Debug"), SerializeField]
        private Text label;

        // Mapping of input IDs to their active gesture tracking objects.
        private readonly Dictionary<int, ActiveGesture> activeGestures = new Dictionary<int, ActiveGesture>();

        /// <summary>
        /// Event fired when the user presses on the screen.
        /// </summary>
        public event Action<SwipeInput> Pressed;

        /// <summary>
        /// Event fired for every motion (possibly multiple times a frame) of a potential swipe gesture.
        /// </summary>
        public event Action<SwipeInput> PotentiallySwiped;
        
        /// <summary>
        /// Event fired for every motion (possibly multiple times a frame) of a drag gesture.
        /// </summary>
        public event Action<DragInput> Dragged;

        /// <summary>
        /// Event fired when a user performs a swipe gesture.
        /// </summary>
        public event Action<SwipeInput> Swiped;

        /// <summary>
        /// Event fired when a user performs a tap gesture, on releasing.
        /// </summary>
        public event Action<TapInput> Tapped;

        public event Action<SwipeInput> DragStarted;
        /// <summary>
        /// Event fired when a user performs a drag gesture, on releasing.
        /// </summary>
        public event Action<DragInput> DragFinished;

        protected virtual void Awake()
        {
            inputManager.Pressed += OnPressed;
            inputManager.Dragged += OnDragged;
            inputManager.Released += OnReleased;
        }

        protected void Update()
        {
            if(activeGestures.Count > 0 && _updateDelayTimer > updateDelay)
            {
                float time = Time.realtimeSinceStartup;

                foreach (ActiveGesture gesture in activeGestures.Values)
                {
                    if(!gesture.IsDragged && time > gesture.StartTime + maxTapDuration)
                    {
                        OnDragStarted(gesture, time);
                    }
                }

                _updateDelayTimer = 0;
            }

            _updateDelayTimer += Time.deltaTime;
        }

        /// <summary>
        /// Checks whether a given active gesture will be a valid swipe.
        /// </summary>
        private bool IsValidSwipe(ref ActiveGesture gesture)
        {
            return gesture.TravelDistance >= minSwipeDistance &&
                (gesture.StartTime - gesture.EndTime) <= maxSwipeDuration &&
                gesture.SwipeDirectionSameness >= swipeDirectionSamenessThreshold;
        }

        /// <summary>
        /// Checks whether a given active gesture will be a valid tap.
        /// 
        /// TODO:  BUG FIXED! IN CASE OF UPDATING InputSystem
        ///        MUST CHECK FOR ERRORS!!
        /// </summary>
        private bool IsValidTap(ref ActiveGesture gesture)
        {
            return gesture.TravelDistance <= maxTapDrift &&
                (gesture.EndTime - gesture.StartTime) <= maxTapDuration;
        }

        private void OnPressed(PointerInput input, double time)
        {
            Debug.Assert(!activeGestures.ContainsKey(input.InputId));

            var newGesture = new ActiveGesture(input.InputId, input.Position, time);

            activeGestures.Add(input.InputId, newGesture);

            DebugInfo(newGesture);

            Pressed?.Invoke(new SwipeInput(newGesture));
        }

        private void OnDragStarted(ActiveGesture gesture, double time)
        {
            gesture.IsDragged = true;

            DebugInfo(gesture);

            DragStarted?.Invoke(new SwipeInput(gesture));
        }

        private void OnDragged(PointerInput input, double time)
        {
            if (!activeGestures.TryGetValue(input.InputId, out var existingGesture))
            {
                // Probably caught by UI, or the input was otherwise lost
                return;
            }

            existingGesture.SubmitPoint(input.Position, time);

            if (existingGesture.TravelDistance < maxTapDrift)
            {
                return;
            }

            if (!existingGesture.IsDragged)
            {
                OnDragStarted(existingGesture, time);
            }

            if (IsValidSwipe(ref existingGesture))
            {
                PotentiallySwiped?.Invoke(new SwipeInput(existingGesture));
            }
            
            Dragged?.Invoke(new DragInput(existingGesture));

            DebugInfo(existingGesture);
        }

        private void OnReleased(PointerInput input, double time)
        {
            if (!activeGestures.TryGetValue(input.InputId, out var existingGesture))
            {
                // Probably caught by UI, or the input was otherwise lost
                return;
            }
            activeGestures.Remove(input.InputId);
            existingGesture.SubmitPoint(input.Position, time);

            existingGesture.IsDragged = false;

            var swipeOrTap = false;
            if (IsValidSwipe(ref existingGesture))
            {
                Swiped?.Invoke(new SwipeInput(existingGesture));
                swipeOrTap = true;
            }

            if (IsValidTap(ref existingGesture))
            {
                Tapped?.Invoke(new TapInput(existingGesture));
                swipeOrTap = true;
            }

            if (!swipeOrTap)
            {
                DragFinished?.Invoke(new DragInput(existingGesture));
            }
            
            DebugInfo(existingGesture);
        }

        private void DebugInfo(ActiveGesture gesture)
        {
            if (label == null) return;

            var builder = new StringBuilder();

            builder.AppendFormat("ID: {0}", gesture.InputId);
            builder.AppendLine();
            builder.AppendFormat("Start Position: {0}", gesture.StartPosition);
            builder.AppendLine();
            builder.AppendFormat("Position: {0}", gesture.EndPosition);
            builder.AppendLine();
            builder.AppendFormat("Duration: {0}", gesture.EndTime - gesture.StartTime);
            builder.AppendLine();
            builder.AppendFormat("Sameness: {0}", gesture.SwipeDirectionSameness);
            builder.AppendLine();
            builder.AppendFormat("Travel distance: {0}", gesture.TravelDistance);
            builder.AppendLine();
            builder.AppendFormat("Samples: {0}", gesture.Samples);
            builder.AppendLine();
            builder.AppendFormat("Realtime since startup: {0}", Time.realtimeSinceStartup);
            builder.AppendLine();
            builder.AppendFormat("Starting Timestamp: {0}", gesture.StartTime);
            builder.AppendLine();
            builder.AppendFormat("Ending Timestamp: {0}", gesture.EndTime);
            builder.AppendLine();

            label.text = builder.ToString();

            var worldStart = camera.ScreenToWorldPoint(gesture.StartPosition);
            var worldEnd = camera.ScreenToWorldPoint(gesture.EndPosition);

            worldStart.z += 5;
            worldEnd.z += 5;
        }
    }
}
