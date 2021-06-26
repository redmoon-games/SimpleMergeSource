namespace Game.Signals
{
    public readonly struct RoomDataChangedSignal : IDataChangedSignal
    {
        public bool IncomeChanged { get; }
        
        public RoomDataChangedSignal(bool incomeChanged = false)
        {
            IncomeChanged = incomeChanged;
        }
    }
}
