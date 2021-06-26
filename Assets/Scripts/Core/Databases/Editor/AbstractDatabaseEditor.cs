using GoogleSheetsToUnity;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Core.Databases.Editor
{
    public abstract class AbstractDatabaseEditor : UnityEditor.Editor
    {
        private IAbstractDatabase _database;

        private void OnEnable()
        {
            _database = (IAbstractDatabase) target;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            
            var downloadButton = new Button() { text = "Download" };

            // Gives it some style.
            downloadButton.style.width = container.contentRect.width;
            downloadButton.style.height = 30;
            downloadButton.style.alignSelf = new StyleEnum<Align>(Align.Center);
            downloadButton.clickable.clicked += UpdateItems;
            container.Add(downloadButton);
            
            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    var propertyField = new PropertyField(iterator.Copy()) { name = "PropertyField:" + iterator.propertyPath };
 
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(value: false);
 
                    container.Add(propertyField);
                }
                while (iterator.NextVisible(false));
            }
 
            return container;
        }

        private void UpdateItems() =>
            SpreadsheetManager.ReadPublicSpreadsheet(
                new GSTU_Search(_database.AssociatedSheet, _database.AssociatedWorksheet), UpdateMethod);

        private void UpdateMethod(GstuSpreadSheet ss)
        {
            _database.UpdateDatabase(ss);
            EditorUtility.SetDirty(target);
        }
    }
}
