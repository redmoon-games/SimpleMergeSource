using Game.Controllers;
using UnityEngine;

namespace Game.Models
{
    public class SpawnUnitContext
    {
        public int level;
        public bool inBox;
        public CellController cellToSpawnIn;

        public SpawnUnitContext(int level, bool inBox = false, CellController cellToSpawnIn = null)
        {
            this.level = level;
            this.inBox = inBox;
            this.cellToSpawnIn = cellToSpawnIn;
        }
    }
}
