using Zenject;
using UnityEngine;
using com.playbux.ability;

namespace com.playbux.networking.server.ability
{
    public abstract class ServerAbilityInstallerBase : MonoInstaller<ServerAbilityInstallerBase>
    {
        [SerializeField]
        private uint abilityId;

        [Inject]
        private AbilityDatabase database;

        public override void InstallBindings()
        {
            if (database.HasKey(abilityId))
            {
                var data = database.Get(abilityId);
                Container.Bind<uint>().FromInstance(abilityId).AsSingle();
                Container.Bind<AbilityData>().FromInstance(data).AsSingle();
            }

            Container.Bind<ServerAbilityFacade>().FromComponentOn(gameObject).AsSingle();
            AbilitySpecificBindings();
        }

        protected abstract void AbilitySpecificBindings();
    }
}