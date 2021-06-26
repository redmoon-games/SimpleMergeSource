using Game.Signals;
using Zenject;

namespace Game.Installers
{
    public class GameSignalsInstaller : Installer<GameSignalsInstaller>
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<MergedSignal>();
            Container.DeclareSignal<MergeChainEndedSignal>();
            Container.DeclareSignal<UnitSignal>();
            Container.DeclareSignal<UnitTappedSignal>();
            Container.DeclareSignal<UnitDataChangedSignal>();
            
            Container.DeclareSignal<ChainEndedSignal>();
            Container.DeclareSignal<RoomChangedSignal>();
            Container.DeclareSignal<RoomDataChangedSignal>();
            Container.DeclareSignal<SpawnMaxReachedSignal>();
            Container.DeclareSignal<SpawnMaxReleasedSignal>();

            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitOpenedSignal>();
            Container.DeclareSignal<WorldDataChangedSignal>();
            Container.DeclareSignal<WorldEndedSignal>();
        }
    }
}
