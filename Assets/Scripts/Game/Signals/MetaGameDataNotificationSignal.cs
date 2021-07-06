using Game.Controllers;

namespace Game.Signals
{
    public readonly struct MetaGameDataNotificationSignal
    {
        public readonly WorldData data;

        public MetaGameDataNotificationSignal(WorldData data)
        {
            this.data = data;
        }
    }
}