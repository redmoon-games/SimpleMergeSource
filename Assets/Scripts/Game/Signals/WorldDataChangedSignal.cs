using Game.Controllers;

namespace Game.Signals
{
    public readonly struct WorldDataChangedSignal : IDataChangedSignal
    {
        public bool IncomeChanged { get; }
        public WorldDataChangedSignal(bool incomeChanged = false)
        {
            IncomeChanged = incomeChanged;
        }
    }
}
