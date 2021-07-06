using Cinemachine;
using Game.Controllers;
using Game.Signals;
using UnityEngine;
using Zenject;

namespace Core.Camera
{
    public class CameraRtView : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera followCamera;
        [SerializeField] private Transform followTarget;

        private CinemachineVirtualCamera _activeCamera;
        private Transform _cameraFollow;
        private Transform _cameraLookAt;
        
        private Vector2 _previousPosition;
        private Vector2 _previousFollowPosition;
        private Vector2 _followPosition;

        private SignalBus _signalBus;
        
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        
        private void OnEnable()
        {
            _signalBus.Subscribe<RoomChangedSignal>(OnRoomChanged);
        }
        
        private void OnDisable()
        {
            _signalBus.Unsubscribe<RoomChangedSignal>(OnRoomChanged);
        }

        private void Awake()
        {
            _activeCamera = followCamera;
            ChangeFollowTarget(followTarget);
        }

        private void ChangeFollowTarget(Transform followTransform)
        {
            _cameraFollow = followTransform;
            _cameraLookAt = followTransform;
            _activeCamera.Follow = _cameraFollow;
            _activeCamera.LookAt = _cameraLookAt;
        }

        private void OnRoomChanged(RoomChangedSignal signal)
        {
            ChangeFollowTarget(signal.Controller.transform);
        }
    }
}