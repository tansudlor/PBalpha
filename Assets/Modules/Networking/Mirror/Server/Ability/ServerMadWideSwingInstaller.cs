using UnityEngine;
using com.playbux.networking.mirror.collision;

namespace com.playbux.networking.server.ability
{
    public class ServerMadWideSwingInstaller : ServerAbilityInstallerBase
    {
        [SerializeField]
        private DonutAreaTrigger triggerArea;

        protected override void AbilitySpecificBindings()
        {
            Container.Bind<IServerAbility>().To<MadWideSwingServerAbility>().AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.Bind<DonutAreaTrigger>().FromInstance(triggerArea).AsSingle();
        }
    }
}