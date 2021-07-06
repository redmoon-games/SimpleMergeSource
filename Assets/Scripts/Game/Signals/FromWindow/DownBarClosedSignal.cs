using Game.UI.BottomBar;

namespace Game.Signals.FromWindow
{
    public readonly struct DownBarClosedSignal
    {
        public EDownBarButtonType ButtonType { get; }

        public DownBarClosedSignal(EDownBarButtonType buttonType)
        {
            ButtonType = buttonType;
        }
    }
}