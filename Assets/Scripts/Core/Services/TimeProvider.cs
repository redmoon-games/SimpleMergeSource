using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.Services
{
    public class TimeProvider : IInitializable, ITickable
    {
        private bool _isPause;
        private float _time;
        private float _timeCoef = 1f;

        public void Initialize()
        {
            //_isPause = true;
        }

        public void Tick()
        {
            if (!_isPause)
            {
                _time += GetDeltaTime();
            }
        }

        public float GetTime() => _time;

        public float GetDeltaTime() => _isPause ? 0f : Time.deltaTime * _timeCoef;

        public bool TogglePause()
        {
            _isPause = !_isPause;
            return _isPause;
        }
    }
}