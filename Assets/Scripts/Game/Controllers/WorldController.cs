using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Databases.Audio;
using Core.Gestures;
using Game.Databases;
using Game.Databases.Implementations;
using Game.Models;
using Game.Services;
using Game.Settings;
using Game.UI;
using Game.Signals;
using UnityEngine;
using Zenject;

namespace Game.Controllers
{
    [Serializable]
    public class WorldData
    {
        public WorldNodeVo node;
        public List<RoomData> rooms;
        public RoomData currentRoom;
        public int spawnLevel;
        public long savedAt;
    }


    public class WorldController : MonoBehaviour
    {
        [SerializeField] private SoundSource musicClip;
        
        [Space]
        [Header("Grid space paddings(%):")]
        [Range(0f, 100f)]
        [SerializeField] private float topPadding = 10f;
        [Range(0f, 100f)]
        [SerializeField] private float bottomPadding = 5f;
        [Range(0f, 100f)]
        [SerializeField] private float leftPadding = 5f;
        [Range(0f, 100f)]
        [SerializeField] private float rightPadding = 5f;

        public IReadOnlyList<RoomData> Rooms => _data.rooms;

        private GridRoomController _currentRoom;
        private readonly Dictionary<GridRoomController, RoomData> _rooms = new Dictionary<GridRoomController, RoomData>();
        private WorldData _data;
        private Rect _viewRect;

        private RoomNodeDatabase _roomNodeDatabase;
        private UnitDatabase _unitDatabase;
        private ResourceDatabase _resourceDatabase;
        private RoomSettings _roomSettings;
        private Camera _camera;
        private MoveManager _moveManager;
        private ProgressManager _progressManager;
        private MergeChainDatabase _mergeChainDatabase;
        private GameSettings _gameSettings;
        private SpawnService _spawnService;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(
            RoomNodeDatabase roomNodeDatabase,
            UnitDatabase unitDatabase,
            ResourceDatabase resourceDatabase,
            RoomSettings roomSettings,
            Camera camera,
            MoveManager moveManager,
            ProgressManager progressManager,
            MergeChainDatabase mergeChainDatabase,
            GameSettings gameSettings,
            SpawnService spawnService,
            SignalBus signalBus
        )
        {
            _roomNodeDatabase = roomNodeDatabase;
            _unitDatabase = unitDatabase;
            _resourceDatabase = resourceDatabase;
            _roomSettings = roomSettings;
            _camera = camera;
            _moveManager = moveManager;
            _moveManager.Swipped += OnSwipe;
            _progressManager = progressManager;
            _mergeChainDatabase = mergeChainDatabase;
            _gameSettings = gameSettings;
            _spawnService = spawnService;
            _signalBus = signalBus;
        }

        private void OnDestroy()
        {
            _moveManager.Swipped -= OnSwipe;
        }

        public void Init(WorldData data)
        {
            _data = data;
            _rooms.Clear();

            SetViewRect();

            if (_data.currentRoom == null)
                InitRoomData();

            InstantiateRooms();
            
            // Subscribe to signals
            _signalBus.Subscribe<ChainEndedSignal>(ChainEndedSignal.CreateRunDelegate(_rooms, OnMergeChainEnded));
            _signalBus.Subscribe<UnitCreatedSignal>(UnitCreatedSignal.CreateRunDelegate(_rooms, OnUnitCreated));

            StartCoroutine(IdleIncomeTimer());
            _signalBus.Fire(new RoomChangedSignal(_currentRoom));
            _signalBus.Fire(new WorldDataChangedSignal(true));
            StartMusic();
        }

        public void UpgradeGrid()
        {
            _currentRoom.UpgradeGrid();
        }
        
        public void UpdateSavedTime(long ticks)
        {
            _data.savedAt = ticks;
        }

        public bool IsAlreadyMax()
        {
            return IsAlreadyMax(_data.spawnLevel);
        }
        
        public bool IsAlreadyMax(int level)
        {
            var room = FindRoomForLevel(level);
            if (room != null)
            {
                return !room.CanAddUnit();
            }

            return true;
        }
        
        public void SpawnUnit()
        {
            SpawnUnit(_data.spawnLevel);
        }
        
        public void SpawnUnit(int level, bool inBox = true)
        {
            var room = FindRoomForLevel(level);
            if (room != null)
            {
                // print($"try SpawnUnit for room {room.Chain.id}");
                room.SpawnUnit(new SpawnUnitContext(level, inBox));
            }
        }

        public void InterruptMove()
        {
            _currentRoom.InterruptMove();
        }

        private void StartMusic()
        {
            musicClip.Play();
        }
        
        private void InstantiateRooms()
        {
            foreach (var roomData in _data.rooms)
            {
                var prefab = _resourceDatabase.GetRoomById(roomData.node.id);
                var room = Instantiate(prefab, transform);
                room.Init(roomData, _viewRect);
                _rooms.Add(room, roomData);
                if (roomData.haveSpawn)
                {
                    var chain = _mergeChainDatabase.GetChainById(roomData.chainId);
                    _spawnService.AddSpawn(roomData.startLevel, chain.nodes.Count - 1);
                }

                if (roomData.chainId == _data.currentRoom.chainId)
                {
                    SetCurrentRoom(room);
                }
            }
        }
        
        private void InitRoomData()
        {
            _data.rooms = new List<RoomData>();
            var roomVo = _roomNodeDatabase.GetRoomByChain(_data.node.firstChainId);
            var room = new RoomData
            {
                node = roomVo,
                startLevel = 0,
                currentLevel = 6,
                chainId = _data.node.firstChainId,
                position = Vector3.zero,
                haveSpawn = true
            };
            _data.rooms.Add(room);
            _data.currentRoom = room;
            var chain = _mergeChainDatabase.GetChainById(room.chainId);
            var unit = _unitDatabase.GetNodeByMergeId(chain.nodes[0]);
            _progressManager.OpenUnit(unit.id);
            if (_data.node.secondChainId != string.Empty)
            {
                var secondChain = _mergeChainDatabase.GetChainById(_data.node.secondChainId);
                var secondMaxLevel = chain.nodes.Count;
                var secondUnit = _unitDatabase.GetNodeByMergeId(secondChain.nodes[0]);
                _progressManager.OpenUnit(secondUnit.id);
                _data.rooms.Add(CreateRoomData(secondMaxLevel, _data.node.secondChainId, true));
                if (_data.node.thirdChainId != string.Empty)
                {
                    var thirdChain = _mergeChainDatabase.GetChainById(_data.node.thirdChainId);
                    var thirdMaxLevel = secondMaxLevel + secondChain.nodes.Count;
                    var thirdUnit = _unitDatabase.GetNodeByMergeId(thirdChain.nodes[0]);
                    _progressManager.OpenUnit(thirdUnit.id);
                    _data.rooms.Add(CreateRoomData(thirdMaxLevel, _data.node.thirdChainId, true));
                }
            }
        }

        private void SetViewRect()
        {
            var leftDown = _camera.ViewportToWorldPoint(
                new Vector3(0, 0, _camera.nearClipPlane)
            );
            var rightUp = _camera.ViewportToWorldPoint(
                new Vector3(1f, 1f, _camera.nearClipPlane)
            );
            var screenSize = rightUp - leftDown;
            
            var workSize = screenSize;
            workSize.z = 0f;
            workSize.y *= 1f - (topPadding + bottomPadding) * .01f;
            workSize.x *= 1f - (leftPadding + rightPadding) * .01f;
            
            var horzOffset = .01f * (rightPadding - leftPadding) * screenSize.x;
            var vertOffset = .01f * (bottomPadding - topPadding) * screenSize.y;
            var pos = new Vector3(horzOffset, vertOffset, 0f);
            
            _viewRect = new Rect(pos, workSize);
        }
        
        private IEnumerator IdleIncomeTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(_gameSettings.incomeSettings.incomeWaveDelay);
                _currentRoom.StartIdleIncome();
            }
        }

        private void OnMergeChainEnded(ChainEndedData data)
        {
            var maxLevel = data.roomController.StartLevel + data.chainVo.nodes.Count;
            var room = FindRoomForLevel(maxLevel);
            if (room != null)
            {
                room.SpawnUnit(new SpawnUnitContext(maxLevel));
            }
            else
            {
                if (data.chainVo.endWith.Count > 0)
                {
                    var newRoom = CreateRoom(maxLevel, data.chainVo.endWith[0]);
                    newRoom.SpawnUnit(new SpawnUnitContext(maxLevel));
                }
                else
                {
                    _signalBus.Fire(new WorldEndedSignal(new WorldEndedData(data.chainVo, this), this));
                }   
            }
        }

        private void SetCurrentRoom(GridRoomController baseRoomController)
        {
            if (_currentRoom != baseRoomController)
            {
                _currentRoom = baseRoomController;
                _data.currentRoom = _rooms[_currentRoom];
                _signalBus.Fire(new RoomChangedSignal(_currentRoom));
                _signalBus.Fire(new WorldDataChangedSignal());
            }
        }

        private void OnUnitCreated(UnitCreatedData data)
        {
            if (!_progressManager.IsUnitOpened(data.data.data.id))
            {
                var list = new List<UnitVo>();
                foreach (var node in data.chainVo.nodes)
                {
                    var unit = _unitDatabase.GetNodeByMergeId(node);
                    list.Add(unit);
                }
                _progressManager.OpenUnit(data.data.data.id);
                _signalBus.Fire(new UnitOpenedSignal(data.data.data, list));
            }
        }
        
        private GridRoomController FindRoomForLevel(int level)
        {
            foreach (var room in _rooms)
            {
                if (room.Key.ContainsLevel(level))
                {
                    return room.Key;
                }
            }

            return null;
        }

        private GridRoomController CreateRoom(int newLevel, string chainId)
        {
            var roomData = CreateRoomData(newLevel, chainId);
            _data.rooms.Add(roomData);
            var prefab = _resourceDatabase.GetRoomById(roomData.node.id);
            var room = Instantiate(prefab, transform);
            room.Init(roomData, _viewRect);
            _rooms.Add(room, roomData);
            SetCurrentRoom(room);
            return room;
        }

        private RoomData CreateRoomData(int newLevel, string chainId, bool haveSpawn = false)
        {
            var roomVo = _roomNodeDatabase.GetRoomByChain(chainId);
            var roomPosition = GetLastRoomPosition() + Vector3.right * _roomSettings.roomPositionShift;
            return new RoomData
            {
                node = roomVo,
                startLevel = newLevel,
                chainId = chainId,
                position = roomPosition,
                haveSpawn = haveSpawn,
                currentLevel = 6
            };
        }
        

        private Vector3 GetLastRoomPosition()
        {
            return _data.rooms[_data.rooms.Count - 1].position;
        }
        
        private void OnSwipe(SwipeInput swipeInput)
        {
            var direction = swipeInput.SwipeDirection;
            if (
                Mathf.Abs(direction.x) > _gameSettings.swipeSettings.minDirectionX && 
                Mathf.Abs(direction.y) < _gameSettings.swipeSettings.maxDirectionY &&
                _rooms.Count > 1 &&
                swipeInput.TravelDistance > _gameSettings.swipeSettings.maxDistance // some threshold to use as a room switch
            )
            {
                print($"[OnSwipe] dir {swipeInput.SwipeDirection.ToString()} dist {swipeInput.TravelDistance}");
                if (direction.x > 0)
                {
                    GotoPreviousRoom();
                }
                else
                {
                    GotoNextRoom();
                }
            }
        }

        private void GotoNextRoom()
        {
            var list = _rooms.ToArray();
            for (var i = 0; i < list.Length; i++)
            {
                var room = list[i];
                if (room.Key == _currentRoom)
                {
                    if (i == _rooms.Count - 1)
                    {
                        SetCurrentRoom(list[0].Key);
                    }
                    else
                    {
                        SetCurrentRoom(list[i + 1].Key);
                    }

                    break;
                }
            }
        }
        
        private void GotoPreviousRoom()
        {
            var list = _rooms.ToArray();
            for (var i = 0; i < list.Length; i++)
            {
                var room = list[i].Key;
                if (room == _currentRoom)
                {
                    if (i == 0)
                    {
                        SetCurrentRoom(list[list.Length - 1].Key);
                    }
                    else
                    {
                        SetCurrentRoom(list[i - 1].Key);
                    }

                    break;
                }
            }
        }
    }
}
