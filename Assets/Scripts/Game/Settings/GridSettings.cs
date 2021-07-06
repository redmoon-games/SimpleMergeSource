using System;
using UnityEngine;

namespace Game.Settings
{
    public static class GridSettings
    {
        public static Vector2Int[] LevelCells = {
            new Vector2Int(0,0),
            new Vector2Int(1,0),
            new Vector2Int(2,0),
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(2,1),
            new Vector2Int(1,2),
            new Vector2Int(2,2),
            new Vector2Int(0,2),
            new Vector2Int(1,3),
            new Vector2Int(2,3),
            new Vector2Int(0,3),
            new Vector2Int(3,2),
            new Vector2Int(3,3),
            new Vector2Int(3,1),
            new Vector2Int(3,0),
            new Vector2Int(1,4),
            new Vector2Int(2,4),
            new Vector2Int(3,4),
            new Vector2Int(0,4),
            new Vector2Int(4,3),
            new Vector2Int(4,2),
            new Vector2Int(4,1),
            new Vector2Int(4,4),
            new Vector2Int(4,0),
            new Vector2Int(2,5),
            new Vector2Int(3,5),
            new Vector2Int(4,5),
            new Vector2Int(1,5),
            new Vector2Int(0,5),
            new Vector2Int(5,3),
            new Vector2Int(5,2),
            new Vector2Int(5,4),
            new Vector2Int(5,1),
            new Vector2Int(5,0),
            new Vector2Int(5,5),
            new Vector2Int(2,6),
            new Vector2Int(3,6),
            new Vector2Int(1,6),
            new Vector2Int(4,6),
            new Vector2Int(5,6),
            new Vector2Int(0,6),
        };

        public static Vector2Int GetGridSizeForLevel(int level)
        {
            var result = new Vector2Int(0, 0);
            for (var i = 0; i <= level; i++)
            {
                result.x = Math.Max(result.x, LevelCells[i].x);
                result.y = Math.Max(result.y, LevelCells[i].y);
            }

            return result + Vector2Int.one;
        }
    }
}