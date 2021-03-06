using Core.Databases.Editor;
using Game.Databases.Implementations;
using UnityEditor;

namespace Editor.Databases.Implementations
{
    [CustomEditor(typeof(BuildingCostDatabase))]
    public class BuildingCostDatabaseEditor  : AbstractDatabaseEditor {}
}