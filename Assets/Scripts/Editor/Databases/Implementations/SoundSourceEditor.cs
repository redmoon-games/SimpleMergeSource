using Core.Databases.Audio;
using Core.Databases.Editor;
using Game.Installers;
using UnityEditor;

namespace Editor.Databases.Implementations
{
    [CustomEditor(typeof(ScriptableInstaller))]
    public class SoundSourceEditor: ScriptableObjectCollector<SoundSource> {}
}