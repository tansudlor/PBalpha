using Zenject;
using UnityEngine;
using com.playbux.io;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.server.ability
{
    public class AbilityInventoryInstaller : MonoInstaller<AbilityInventoryInstaller>
    {
        [SerializeField]
        private Object jsonIOInstaller;

        [SerializeField]
        private FileInfo localAbilityInventoryPath;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<AbilityInventory>().AsSingle();
            Container.Bind<FileInfo>().FromInstance(localAbilityInventoryPath).AsSingle();
            Container.Bind<IOFacade<SerializedDictionary<string, uint[]>>>().FromSubContainerResolve().ByNewContextPrefab(jsonIOInstaller).AsSingle();
        }
    }
}