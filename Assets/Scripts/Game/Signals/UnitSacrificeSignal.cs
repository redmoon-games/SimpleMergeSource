namespace Game.Signals
{
    public readonly struct UnitSacrificeSignal
    {
        public readonly string UnitId;

        public UnitSacrificeSignal(string unitId)
        {
            UnitId = unitId;
        }
    }
}