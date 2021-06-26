using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Models;
using UnityEngine;
using DG.Tweening;
using Game.Databases;
using Game.Databases.Implementations;
using Game.Merge;
using Game.Settings;
using Game.UI;
using Game.Signals;
using Game.UI.DeleteUnitIcon;
using Utils;
using Zenject;
using Random = UnityEngine.Random;

namespace Game.Controllers
{
    [Serializable]
    public class RoomData
    {
        public RoomNodeVo node;
        public List<UnitData> units;
        public int startLevel;
        public IntRange levelRange;
        public int currentLevel;
        public string chainId;
        public Vector3 position;
        public bool haveSpawn;
    }

    public class GridRoomController : MonoBehaviour
    {
        [SerializeField] private GridController gridController;
        [SerializeField] private Sprite boxSprite;

        public int StartLevel => _data.startLevel;
        public int MaxLevel => _data.levelRange.max;

        public MergeChainVo Chain => _chainVo;

        private RoomData _data;
        private MergeChainVo _chainVo;
        private readonly Dictionary<UnitController, UnitData> _units = new Dictionary<UnitController, UnitData>();
        private List<UnitController> _idleIncomeUnits = new List<UnitController>();
        private readonly Dictionary<UnitController, CellController> _cells = new Dictionary<UnitController, CellController>();
        private readonly List<CellController> _freeCells = new List<CellController>();
        private CellController _selectedCell;

        private MergeChainDatabase _mergeChainDatabase;
        private MergeNodeDatabase _mergeNodeDatabase;
        private UnitDatabase _unitDatabase;
        private ResourceDatabase _resourceDatabase;
        private IncomeSettings _incomeSettings;
        private SignalBus _signalBus;
        private DeleteUnit _deleteUnit;

        [Inject]
        public void Construct(
            MergeChainDatabase mergeChainDatabase,
            MergeNodeDatabase mergeNodeDatabase,
            UnitDatabase unitDatabase,
            ResourceDatabase resourceDatabase,
            SpawnSettings spawnSettings,
            IncomeSettings incomeSettings,
            SignalBus signalBus,
            DeleteUnit deleteUnit
            )
        {
            _mergeChainDatabase = mergeChainDatabase;
            _mergeNodeDatabase = mergeNodeDatabase;
            _unitDatabase = unitDatabase;
            _resourceDatabase = resourceDatabase;
            _incomeSettings = incomeSettings;
            _signalBus = signalBus;
            _deleteUnit = deleteUnit;
        }

        public void Init(RoomData data, Rect viewRect)
        {
            _data = data;
            transform.position = _data.position;
            var roomViewRect = viewRect;
            roomViewRect.position += (Vector2)data.position;
            gridController.Init(roomViewRect, data.currentLevel);
            _chainVo = _mergeChainDatabase.GetChainById(data.chainId);
            _data.levelRange = new IntRange(_data.startLevel, _data.startLevel + _chainVo.nodes.Count);
            if (_data.units != null)
            {
                foreach (var unitData in _data.units)
                {
                    var prefab = _resourceDatabase.GetUnitById(unitData.data.id);
                    var unit = Instantiate(prefab, transform);
                    var mergeVo = _mergeNodeDatabase.GetNodeById(unitData.data.mergeNodeId);

                    var cell = gridController.FindCellByPosition(unitData.gridPosition);

                    unit.Init(new UnitInitParams
                    {
                        data = unitData,
                        mergeNodeVo = mergeVo,
                        roomData = _data,
                        boxSprite = boxSprite
                    });

                    AddUnitToCell(unit, cell);
                    _units.Add(unit, unitData);
                }
            }
            else
            {
                _data.units = new List<UnitData>();
            }

            _freeCells.Clear();
            _freeCells.AddRange(gridController.Cells);
            foreach (var pair in _cells)
            {
                _freeCells.Remove(pair.Value);
            }

            // Subscribe to signals
            _signalBus.Subscribe<MergeChainEndedSignal>(MergeChainEndedSignal.CreateRunDelegate(_units, OnMergeChainEnded));
            _signalBus.Subscribe<MergedSignal>(MergedSignal.CreateRunDelegate(_units, OnMerged));
            _signalBus.Subscribe<UnitSignal>(UnitSignal.CreateRunDelegate(_units, OnUnitSignal));
        }

        public bool ContainsLevel(int level)
        {
            return _data.levelRange.Contains(level);
        }

        public void InterruptMove()
        {
            foreach (var unit in _units.Keys)
            {
                unit.InterruptMove();
            }
        }

        public void StartIdleIncome()
        {
            _idleIncomeUnits = _units.Keys.ToList();
            StartCoroutine(IdleIncome());
        }

        public bool CanAddUnit()
        {
            return _freeCells.Count > 0;
        }

        public void SpawnUnit(SpawnUnitContext context)
        {
            var unitCell = context.cellToSpawnIn != null ? context.cellToSpawnIn : GetRandomCellInRoom();

            var mergeId = _chainVo.Get(context.level - _data.startLevel);
            var mergeVo = _mergeNodeDatabase.GetNodeById(mergeId);
            var unitVo = _unitDatabase.GetNodeByMergeId(mergeId);
            var unitData = new UnitData
            {
                data = unitVo,
                state = context.inBox ? ECharacterState.InBox : ECharacterState.Idle,
                level = context.level
            };
            var unitController = InstantiateUnit(mergeVo, unitData, context);
            AddUnitToCell(unitController, unitCell);
            unitData.gridPosition = unitCell.GridCoord;
            _data.units.Add(unitData);
            _signalBus.Fire(new UnitCreatedSignal(new UnitCreatedData(unitData, _chainVo), this));

            if (!CanAddUnit()) _signalBus.Fire(new SpawnMaxReachedSignal(new SpawnMaxReachedData(context.level), this));

            _signalBus.Fire(new RoomDataChangedSignal(!context.inBox));
        }

        private CellController GetRandomCellInRoom()
        {
            var cell = _freeCells[Random.Range(0, _freeCells.Count - 1)];
            return cell;
        }

        private void MakeMergeAnim(UnitController source, UnitController destination, Action<CellController> onComplete)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(source.transform.DOMove(destination.transform.position, 0.2f).OnComplete(() =>
            {
                OnAnimComplete(source, destination, onComplete);
            }));
        }

        private void OnAnimComplete(UnitController source, UnitController destination, Action<CellController> onComplete)
        {
            var sourceCell = _cells[source];
            var destinationCell = _cells[destination];
            RemoveUnitFromCell(source, sourceCell);
            RemoveUnitFromCell(destination, destinationCell);

            Destroy(source.gameObject);
            Destroy(destination.gameObject);

            onComplete(destinationCell);

            if (CanAddUnit())
            {
                _signalBus.Fire(new SpawnMaxReleasedSignal(new SpawnMaxReleasedData(_data.startLevel), this));
            }
        }

        private void OnUnitSignal(UnitSignalData data)
        {
            switch (data.type)
            {
                case EUnitSignalType.Dragged:
                    OnUnitDragged(data.unitRef);
                    break;
                case EUnitSignalType.Moved:
                    OnUnitMoved(data.unitRef);
                    break;
                case EUnitSignalType.StartDragged:
                    OnUnitStartDragged(data.unitRef);
                    break;
            }
        }

        private void OnUnitMoved(UnitController unit)
        {
            gridController.ClearCellsEffects();

            if (_selectedCell != null)
            {
                if (_deleteUnit.CanDelete)
                {
                    _idleIncomeUnits.Remove(unit);
                    _data.units.Remove(_units[unit]);
                    RemoveUnitFromCell(unit, _cells[unit]);
                    _units.Remove(unit);
                    Destroy(unit.gameObject);
                    _deleteUnit.CanDelete = false;
                    _signalBus.Fire(new SpawnMaxReleasedSignal(new SpawnMaxReleasedData(_data.startLevel), this));
                    _signalBus.Fire(new RoomDataChangedSignal(true));
                    return;
                }

                if (IsCellFree(_selectedCell))
                {
                    var unitCell = _cells[unit];

                    RemoveUnitFromCell(unit, unitCell);
                    AddUnitToCell(unit, _selectedCell);
                    return;
                }

                if (IsCellMergeable(_selectedCell, unit, out var cellUnit))
                {
                    unit.Mergeable.Merge(cellUnit.Mergeable);
                    return;
                }
            }
            unit.ResetLocalPosition();
        }

        private void OnUnitDragged(UnitController unit)
        {
            var newCell = FindNearestCell(unit.transform.position, unit);

            if (newCell != _selectedCell && _selectedCell != null)
            {
                _selectedCell.SetSelected(false);
                _selectedCell = newCell;
                _selectedCell.SetSelected(true);
            }
        }

        private void OnUnitStartDragged(UnitController unit)
        {
            _selectedCell = FindNearestCell(unit.transform.position, unit);
            _selectedCell.SetSelected(true);

            foreach (var cellController in gridController.Cells)
            {
                if (IsCellMergeable(cellController, unit))
                {
                    cellController.SetMergeable(true);
                }
            }
        }

        private void RemoveUnitFromCell(UnitController unit, CellController cell)
        {
            _cells.Remove(unit);
            _freeCells.Add(cell);
        }

        private void AddUnitToCell(UnitController unit, CellController cell)
        {
            _freeCells.Remove(cell);
            _cells.Add(unit, cell);
            unit.transform.SetParent(cell.transform);
            unit.SetLocalPosition(Vector2.zero, cell.GridCoord);
            unit.transform.localScale = Vector2.one;
        }

        private bool IsCellFree(CellController cell) => _freeCells.Contains(cell);

        private bool IsCellMergeable(CellController cell, UnitController unit)
        {
            return IsCellMergeable(cell, unit, out var cellUnit);
        }

        private bool IsCellMergeable(CellController cell, UnitController unit, out UnitController cellUnit)
        {
            if (FindUnitByCell(cell, out cellUnit))
            {
                return cellUnit.Mergeable.CanMerge(unit.Mergeable);
            }
            return false;
        }

        private bool FindUnitByCell(CellController cell, out UnitController unit)
        {
            if (_cells.ContainsValue(cell))
            {
                foreach (var keyUnit in _cells.Keys)
                {
                    if (_cells[keyUnit] == cell)
                    {
                        unit = keyUnit;
                        return true;
                    }
                }
            }
            unit = null;
            return false;
        }

        private CellController FindNearestCell(Vector2 position, UnitController unit)
        {
            var cells = gridController.SortForPosition(position);

            if (cells != null)
            {
                foreach (var cell in cells)
                {
                    if (IsCellFree(cell) || IsCellMergeable(cell, unit) || _cells[unit] == cell)
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        public void UpgradeGrid()
        {
            var newCell = gridController.UpgradeGrid();
            _freeCells.Add(newCell);
            _data.currentLevel = gridController.GridLevel;

            _signalBus.Fire(new SpawnMaxReleasedSignal(new SpawnMaxReleasedData(_data.startLevel), this));
            _signalBus.Fire(new RoomDataChangedSignal());
        }

        private UnitController InstantiateUnit(MergeNodeVo mergeVo, UnitData data, SpawnUnitContext context)
        {
            var unitVo = data.data;
            var prefab = _resourceDatabase.GetUnitById(unitVo.id);
            var unit = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);

            unit.Init(new UnitInitParams
            {
                data = data,
                mergeNodeVo = mergeVo,
                roomData = _data,
                boxSprite = boxSprite
            });

            _units.Add(unit, data);
            return unit;
        }

        private void OnMergeChainEnded(MergedData data)
        {
            Merge(data, cell =>
            {
                _signalBus.Fire(new ChainEndedSignal(new ChainEndedData(data.chain, this), this));
            });
        }

        private void OnMerged(MergedData data)
        {
            Merge(data, cell =>
            {
                SpawnUnit(new SpawnUnitContext(
                    _data.startLevel + data.NextNode(),
                    false,
                    cell
                ));
            });
        }

        private void Merge(MergedData data, Action<CellController> onComplete)
        {
            var source = data.source.GetComponent<UnitController>();
            var destination = data.destination.GetComponent<UnitController>();

            source.ChangeState(ECharacterState.Merge);
            destination.ChangeState(ECharacterState.Merge);

            _data.units.Remove(_units[source]);
            _data.units.Remove(_units[destination]);
            
            _units.Remove(source);
            _units.Remove(destination);

            MakeMergeAnim(source, destination, onComplete);
        }

        private IEnumerator IdleIncome()
        {
            while (_idleIncomeUnits.Count > 0)
            {
                var randomUnit = _idleIncomeUnits[Random.Range(0, _idleIncomeUnits.Count)];
                randomUnit.Tap();

                _idleIncomeUnits.Remove(randomUnit);
                yield return new WaitForSeconds(Random.Range(_incomeSettings.incomeDelayMin, _incomeSettings.incomeDelayMax));
            }
        }
    }
}
