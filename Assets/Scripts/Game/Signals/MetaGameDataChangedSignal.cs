namespace Game.Signals
{
    public readonly struct MetaGameDataChangedSignal : IDataChangedSignal
    {
        public bool IncomeChanged { get; }

        public MetaGameDataChangedSignal(bool incomeChanged)
        {
            IncomeChanged = incomeChanged;
        }
    }
}