using GoogleSheetsToUnity;
using UnityEngine;

namespace Core.Databases
{
    public abstract class AbstractDatabase<T> : ScriptableObject, IAbstractDatabase
    {
        [SerializeField] protected T data;

        public virtual string AssociatedSheet => "1N-6Hi_GOLJvHGHKVM8Ds_KhLyzEu81mBLYexn8L5FzY";
        public abstract string AssociatedWorksheet { get; }

        public T GetData() => data;

        public abstract void UpdateDatabase(GstuSpreadSheet ss);
    }
}
