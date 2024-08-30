using com.playbux.networking.mirror.collision;
using UnityEngine;

namespace com.playbux.networking.server.ability
{
    public class ServerMadTightSwingInstaller : ServerAbilityInstallerBase
    {
        [SerializeField]
        private CircleAreaTrigger triggerArea;

        protected override void AbilitySpecificBindings()
        {
            Container.Bind<IServerAbility>().To<MadTightSwingServerAbility>().AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.Bind<CircleAreaTrigger>().FromInstance(triggerArea).AsSingle();
        }
    }
}