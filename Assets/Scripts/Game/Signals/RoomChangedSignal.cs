using Game.Controllers;

namespace Game.Signals
{
    public readonly struct RoomChangedSignal
    {
        public GridRoomController Controller { get; }
        
        public RoomChangedSignal(GridRoomController controller)
        {
            Controller = controller;
        }
    }
}
