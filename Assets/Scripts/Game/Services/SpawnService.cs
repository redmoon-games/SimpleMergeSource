using System.Collections.Generic;
using Game.Controllers;
using Game.Settings;
using Game.Signals;
using Game.UI;
using UnityEngine;
using Utils;
using Zenject;

namespace Game.Services
{
    public class SpawnTimer
    {
        public Timer timer = new Timer();
        public int startLevel;
        public int count;
        public bool needSpawn = false;
    }
    public class SpawnService : MonoBehaviour
    {
        [SerializeField] private RootController root;
        [SerializeField] private SpawnButton spawnButton;

        private readonly List<SpawnTimer> _timers = new List<SpawnTimer>();

        private int _CurrentStartLevel;
        
        private SpawnSettings _settings;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SpawnSettings settings, SignalBus signalBus)
        {
            _settings = settings;
            _signalBus = signalBus;
        }

        public void AddSpawn(int startLevel, int count)
        {
            var timer = new SpawnTimer();
            timer.timer.onTimerFinished += () => OnTimerFinished(timer);
            timer.startLevel = startLevel;
            timer.count = count;
            timer.timer.Init(0);
            _timers.Add(timer);
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<SpawnMaxReachedSignal>(OnSpawnMaxReached);
            _signalBus.Subscribe<SpawnMaxReleasedSignal>(OnSpawnMaxReleased);
            _signalBus.Subscribe<RoomChangedSignal>(OnRoomChanged);
            spawnButton.ButtonTaped += OnButtonTapped;

            spawnButton.SetMaxTime(_settings.startSpawnTime);
        }

        private void OnDisable()
        {
            _signalBus.Unsubscribe<SpawnMaxReachedSignal>(OnSpawnMaxReached);
            _signalBus.Unsubscribe<SpawnMaxReleasedSignal>(OnSpawnMaxReleased);
            _signalBus.Unsubscribe<RoomChangedSignal>(OnRoomChanged);
            spawnButton.ButtonTaped -= OnButtonTapped;
        }

        private void Update()
        {
            var delta = Time.deltaTime;
            foreach (var timer in _timers)
            {
                timer.timer.Update(delta);
                if(timer.startLevel == _CurrentStartLevel)
                {
                    spawnButton.SetMaxTime(_settings.startSpawnTime);
                    spawnButton.SetFillAmount(timer.timer.TimerFillAmount);
                }
            }
        }

        private void OnTimerFinished(SpawnTimer timer)
        {
            if (!root.WorldController.IsAlreadyMax(timer.startLevel))
            {
                FinishTimer(timer);
            }
            else
            {
                if (timer.startLevel == _CurrentStartLevel)
                {
                    spawnButton.SetIsRoomFull(true);
                }
                timer.needSpawn = true;
            }
        }
        
        private void OnSpawnMaxReached(SpawnMaxReachedSignal signal)
        {

        }
        
        private void OnSpawnMaxReleased(SpawnMaxReleasedSignal signal)
        {
            var timer = FindForLevel(signal.Value.level);
            if (timer.needSpawn)
            {
                timer.needSpawn = false;

                if (!root.WorldController.IsAlreadyMax(timer.startLevel))
                {
                    if (timer.startLevel == _CurrentStartLevel)
                    {
                        spawnButton.SetIsRoomFull(false);
                    }

                    FinishTimer(timer);
                }
            }
        }

        private void OnRoomChanged(RoomChangedSignal signal)
        {
            var timer = FindForLevel(signal.Controller.StartLevel);

            if(timer != null)
            {
                _CurrentStartLevel = signal.Controller.StartLevel;
                spawnButton.SetFillAmount(timer.timer.TimerFillAmount);

                spawnButton.SetIsRoomFull(root.WorldController.IsAlreadyMax(timer.startLevel));
            }
        }

        private void OnButtonTapped(float maxTime)
        {
            var timer = FindForLevel(_CurrentStartLevel);

            if(timer != null)
            {
                float seconds = timer.timer.TimerFillAmount * maxTime;
                int targetSeconds = Mathf.RoundToInt(seconds + 1);

                float updateSeconds = targetSeconds - seconds;
                timer.timer.Update(updateSeconds);
            }
        }

        private void FinishTimer(SpawnTimer timer)
        {
            root.WorldController.SpawnUnit(timer.startLevel);
            // TODO: save/load this value or the global struct with game progress
            timer.timer.Reset(_settings.startSpawnTime);
        }

        private SpawnTimer FindForLevel(int level)
        {
            foreach (var timer in _timers)
            {
                if (level >= timer.startLevel && level < timer.startLevel + timer.count)
                {
                    return timer;
                }
            }

            return null;
        }
    }
}
