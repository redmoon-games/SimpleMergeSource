using System;
using System.Collections.Generic;
using DG.Tweening;
using Game.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Controllers
{
    public class GridController : MonoBehaviour
    {
        [SerializeField] private int level;

        [Header("Cell spacing(%):")] [Range(0f, 100f)] [SerializeField]
        private float cellSpacing = 5f;

        [Space] [SerializeField]
        private Grid grid;
        [SerializeField] private CellController prefabCell;

        [Space] [SerializeField]
        private float upgradeAnimationDuration;

        public List<CellController> Cells { get; } = new List<CellController>();

        private Rect _viewRect;
        private Vector2Int _gridSize;

        public int GridLevel => level;

        private Vector2 GridSize => new Vector2((grid.cellSize.x + grid.cellGap.x) * _gridSize.x,
            (grid.cellSize.y + grid.cellGap.y) * _gridSize.y);

        public void Init(Rect viewRect, int gridLevel)
        {
            level = gridLevel;
            _viewRect = viewRect;
            _gridSize = GridSettings.GetGridSizeForLevel(level - 1);
            var offset = ResizeGrid();

            Cells.Clear();
            for (var i = 0; i < level; i++)
            {
                var cellPos = GridSettings.LevelCells[i];
                var localPos = grid.GetCellCenterLocal((Vector3Int)cellPos);

                var cell = Instantiate(prefabCell, localPos, Quaternion.identity, transform);
                cell.Init(IsLightCell(cellPos), cellPos);
                cell.SetScale(grid.cellSize);
                Cells.Add(cell);
            }
            offset.y = 0;
            transform.position = _viewRect.position - GridSize / 2 - offset;
            RefreshCellsScaleAndPosition(false);
        }

        private Vector2 ResizeGrid()
        {
            var cellWidth = _viewRect.width / _gridSize.x;
            var cellHeight = _viewRect.height / _gridSize.y;
            var minSize = Math.Min(cellWidth, cellHeight);
            var cellGap = minSize * cellSpacing * .01f;
            var cellSize = minSize - cellGap;
            grid.cellGap = new Vector2(cellGap, cellGap);
            grid.cellSize = new Vector2(cellSize, cellSize);

            var offsetX = (cellWidth - minSize) * _gridSize.x * .5f;
            var offsetY = (cellHeight - minSize) * _gridSize.y * .5f;
            return new Vector2(offsetX, offsetY);
        }

        private static bool IsLightCell(Vector2Int coord) =>
            coord.x % 2 == 0 ? (coord.y + 1) % 2 == 0 : coord.y % 2 == 0;

        public CellController FindCellByPosition(Vector2Int position)
        {
            foreach (var cell in Cells)
            {
                if (cell.GridCoord == position)
                {
                    return cell;
                }
            }

            return null;
        }

        public List<CellController> SortForPosition(Vector2 position)
        {
            if (Cells.Count > 0)
            {
                var cells = new List<CellController>(Cells);

                cells.Sort((c1, c2) =>
                {
                    var distance1 = Vector2.Distance(c1.transform.position, position);
                    var distance2 = Vector2.Distance(c2.transform.position, position);
                    return distance1.CompareTo(distance2);
                });

                return cells;
            }

            return null;
        }

        public void ClearCellsEffects()
        {
            foreach (var cellController in Cells)
            {
                cellController.SetMergeable(false);
                cellController.SetSelected(false);
            }
        }

        public CellController UpgradeGrid()
        {
            _gridSize = GridSettings.GetGridSizeForLevel(level);
            var offset = ResizeGrid();
            var newCell = CreateCell();
            level++;
            RefreshCellsScaleAndPosition(true, true);
            offset.y = 0;
            var newPosition = _viewRect.position - GridSize / 2 - offset;
            transform.DOMove(newPosition, upgradeAnimationDuration);
            newCell.AnimateCreation();
            return newCell;
        }

        private void RefreshCellsScaleAndPosition(bool upgrade, bool animate = false)
        {
            for (var i = 0; i < level; i++)
            {
                if (upgrade && i == level - 1 || !animate)
                {
                    Cells[i].transform.localScale = grid.cellSize;
                    Cells[i].transform.localPosition = grid.GetCellCenterLocal((Vector3Int)GridSettings.LevelCells[i]);
                }
                else
                {
                    var sequence = DOTween.Sequence();
                    sequence.Append(Cells[i].transform.DOScale(grid.cellSize.x, upgradeAnimationDuration));
                    sequence.Join(Cells[i].transform
                        .DOLocalMove(grid.GetCellCenterLocal((Vector3Int)GridSettings.LevelCells[i]),
                            upgradeAnimationDuration));
                }
            }
        }

        private CellController CreateCell()
        {
            var cellPos = GridSettings.LevelCells[level];
            var localPos = grid.GetCellCenterLocal((Vector3Int)cellPos);

            var cell = Instantiate(prefabCell, localPos, Quaternion.identity, transform);
            cell.Init(IsLightCell(cellPos), cellPos);
            Cells.Add(cell);
            return cell;
        }
    }
}
