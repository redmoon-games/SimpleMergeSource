using Core.Databases.Editor;
using Game.Databases.Implementations;
using UnityEditor;

namespace Editor.Databases.Implementations
{
    [CustomEditor(typeof(MergeChainDatabase))]
    public class MergeChainDatabaseEditor : AbstractDatabaseEditor {}
}
