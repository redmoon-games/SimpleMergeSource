using System;
using System.Collections.Generic;
using Core.Localization;
using GoogleSheetsToUnity;
using UnityEngine;
using Utils;

namespace Core.Databases
{
    [Serializable]
    public class LocalizeSaveData
    {
        public string fieldName;
        public string language;
        public string value;
    }
    
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Scriptable Database/Localization")]
    public class LocalizationDatabase : AbstractDatabase<List<LocalizeModel>>, ISerializationCallbackReceiver
    {
        public override string AssociatedSheet => "1NXDM84o6WvYb5nZ_8srQrUpGe9IsQww8ntTmurgOP3Y";
        public override string AssociatedWorksheet => "Data";
        
        [SerializeField] private List<LocalizeSaveData> _localizeSave = new List<LocalizeSaveData>();

        public string GetLocalizeText(string fieldName, string language = "Default") => 
            data.GetBy(value => value.fieldName == fieldName).values[language];
        
        public override void UpdateDatabase(GstuSpreadSheet ss)
        {
            data = new List<LocalizeModel>();
            foreach (var cell in ss.columns["Key"])
            {
                if (cell.value == "Key")
                    continue;
                
                if (cell.value == "--")
                    break;

                var row = ss.rows[cell.value];
                var model = new LocalizeModel() {fieldName = cell.value};
                foreach (var rowCell in row)
                {
                    if (rowCell.value.Trim() == string.Empty)
                        continue;

                    if(rowCell.columnId == "Key")
                        continue;

                    if (!model.values.ContainsKey(rowCell.columnId))
                    {
                        model.values.Add(rowCell.columnId, rowCell.value);
                    }
                }
                data.Add(model);
            }
        }

        public void OnBeforeSerialize()
        {
            _localizeSave.Clear();
            foreach (var model in data)
            {
                foreach (var modelValue in model.values)
                {
                    _localizeSave.Add(new LocalizeSaveData()
                    {
                        fieldName = model.fieldName,
                        language = modelValue.Key,
                        value = modelValue.Value
                    });
                }
            }
        }

        public void OnAfterDeserialize()
        {
            data = new List<LocalizeModel>();
            foreach (var save in _localizeSave)
            {
                var model = data.GetBy(value => value.fieldName == save.fieldName);
                if (model != default)
                {
                    if (!model.values.ContainsKey(save.language))
                    {
                        model.values.Add(save.language, save.value);
                    }
                }
                else
                {
                    var newModel = new LocalizeModel() {fieldName = save.fieldName};
                    newModel.values.Add(save.language, save.value);
                    data.Add(newModel);
                }
            }
        }
    }
}
