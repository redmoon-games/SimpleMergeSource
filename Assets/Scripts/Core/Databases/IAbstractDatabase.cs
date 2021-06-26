using GoogleSheetsToUnity;

namespace Core.Databases
{
    public interface IAbstractDatabase
    {
        public void UpdateDatabase(GstuSpreadSheet ss);
        public string AssociatedSheet { get; }
        public string AssociatedWorksheet { get; }
    }
}