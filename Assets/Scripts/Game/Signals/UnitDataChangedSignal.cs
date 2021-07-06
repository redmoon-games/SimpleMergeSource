namespace Game.Signals
{
    public readonly struct UnitDataChangedSignal : IDataChangedSignal
    {
        public bool IncomeChanged { get; }
        
        public UnitDataChangedSignal(bool incomeChanged = false)
        {
            IncomeChanged = incomeChanged;
        }
    }
}
