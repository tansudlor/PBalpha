using System.Collections.Generic;
using UnityEngine;
using Zenject;
using com.playbux.inventory;
using com.playbux.networking.mirror.message;
using com.playbux.events;

namespace com.playbux.networking.networkinventory
{
    [CreateAssetMenu(menuName = "Playbux/Create Inventory Installer", fileName = "InventoryInstaller")]
    public class InventoryInstaller : ScriptableObjectInstaller
    {
        public TextAsset NFTDatabase;
        public TextAsset[] TestData;
        public GameObject InventoryView;
        public override void InstallBindings()
        {
            Container.BindInstance<TextAsset[]>(TestData);
            Container.BindInstance<TextAsset>(NFTDatabase);
            Container.Bind<NetworkInventoryModel>().To<NetworkInventoryModel>().AsSingle().NonLazy();
#if !SERVER
            BindClientSide();

#endif

#if SERVER
            BindServerSide();
#endif
        }


#if SERVER
        private void BindServerSide()
        {
            // Container.Bind<IEntityBehaviour>().To<AvatarServer>().AsSingle();
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<InventoryUpdateMessage>))
                .To<ServerNetworkMessageReceiver<InventoryUpdateMessage>>().AsSingle();
        }
#endif


#if !SERVER
        private void BindClientSide()
        {
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<InventoryUpdateMessage>))
                .To<ClientMessageReceiver<InventoryUpdateMessage>>().AsSingle();
            Container.Bind<InventoryUIController>().FromSubContainerResolve().ByNewContextPrefab(InventoryView).AsSingle().OnInstantiated<InventoryUIController>((context, controller) =>
            {
                controller.enabled = true;
            }).NonLazy();

            Container.BindSignal<ConversationDialogSignal>().ToMethod<InventoryUIController>(controller => controller.OnConversationSignalReceived).FromResolveAll();
        }
#endif
    }
}
