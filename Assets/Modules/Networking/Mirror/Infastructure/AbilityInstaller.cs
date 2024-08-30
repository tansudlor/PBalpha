using Zenject;
using UnityEngine;
using com.playbux.ability;
using com.playbux.networking.client.ability;
using com.playbux.networking.mirror.message;
using com.playbux.networking.server.ability;

namespace com.playbux.networking.mirror.infastructure
{
    public class AbilityInstaller : MonoInstaller<AbilityInstaller>
    {
        [SerializeField]
        private AbilityDatabase abilityDatabase;

        [SerializeField]
        private AbilityAssetDatabase clientAssetDatabase;

        [SerializeField]
        private AbilityAssetDatabase serverAssetDatabase;

        [SerializeField]
        private AbilityInventoryInstaller abilityInventoryInstaller;

        public override void InstallBindings()
        {
            Container.Bind<AbilityDatabase>().FromInstance(abilityDatabase).AsSingle();
            Container.Bind<AbilityAssetDatabase>().FromMethod(GetAssetDatabase).AsSingle();

#if !SERVER
            Container.BindFactory<Object, Vector3, IClientAbility, IClientAbility.Factory>().FromFactory<ClientAbilityFactory>();
            Container.BindInterfacesAndSelfTo<ClientAbilityController>().AsSingle();
            BindClientMessage();
#else
            BindServerMessage();
            Container.BindFactory<GameObject, Vector2, ServerAbilityFacade, AbilityServerFactory>().FromFactory<ServerAbilityFactory>();
            Container.Bind<AbilityInventory>().FromSubContainerResolve().ByNewContextPrefab(abilityInventoryInstaller).AsSingle().NonLazy();
#endif
        }

        private AbilityAssetDatabase GetAssetDatabase()
        {
#if SERVER
            return serverAssetDatabase;
#else
            return clientAssetDatabase;
#endif
        }


#if !SERVER
        private void BindClientMessage()
        {
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<EndCastMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<StartCastMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<UpdateCastMessage>>().AsSingle();
            Container.BindInterfacesAndSelfTo<ClientMessageReceiver<CancelCastMessage>>().AsSingle();
        }
#else
        private void BindServerMessage()
        {
            // Container.BindInterfacesAndSelfTo<ServerNetworkMessageReceiver<AbilitySpawnMessage>>().AsSingle();
        }
#endif
    }
}