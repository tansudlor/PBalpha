using UnityEngine;
using Zenject;

namespace com.playbux.network.api.rest
{
    public class PackApiInstaller : MonoInstaller<PackApiInstaller>
    {
        [SerializeField]
        private GameObjectContext[] contexts;
        
        public override void InstallBindings()
        {
            Container.Bind<PackApiFacade>().AsSingle();
            
            foreach (var context in contexts)
                Container.Bind<IDataStore>().FromSubContainerResolve().ByNewContextPrefab(context).AsTransient();
        }
    }
}