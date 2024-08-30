using com.playbux.ability;
using UnityEngine;
using Zenject;
namespace com.playbux.networking.client.ability
{
    public abstract class ClientAbilityInstallerBase<T> : MonoInstaller<ClientAbilityInstallerBase<T>> where T : IClientAbility
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

            Container.Bind<IClientAbility>().To<T>().FromComponentOn(gameObject).AsSingle();
            AbilitySpecificBindings();
        }

        protected abstract void AbilitySpecificBindings();
    }
}