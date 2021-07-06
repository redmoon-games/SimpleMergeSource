using Game.UI.BottomBar;

namespace Game.Signals.FromWindow
{
    public readonly struct DownBarShowedSignal
    {
        public EDownBarButtonType ButtonType { get; }

        public DownBarShowedSignal(EDownBarButtonType buttonType)
        {
            ButtonType = buttonType;
        }
    }
}