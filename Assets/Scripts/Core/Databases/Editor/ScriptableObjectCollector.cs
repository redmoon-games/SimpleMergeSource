using Game.Databases.Helpers;
using Game.Installers;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.Databases.Editor
{
    public class ScriptableObjectCollector<T>: UnityEditor.Editor where T : ScriptableObject
    {
        private ICollectable<T> _data;

        private void OnEnable()
        {
            _data = (ICollectable<T>) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            var downloadButton = new Button {text = "Download"};

            // Gives it some style.
            downloadButton.style.width = container.contentRect.width;
            downloadButton.style.height = 30;
            downloadButton.style.alignSelf = new StyleEnum<Align>(Align.Center);
            downloadButton.clickable.clicked += SetCollectedItems;
            container.Add(downloadButton);
            
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
 
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(false);
 
                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }

            return container;
        }

        private void SetCollectedItems()
        {
            _data.SetData(ScriptableObjectHelpers.GetAllInstances<T>(_data.GetRootFolder()));
        }

    }
}