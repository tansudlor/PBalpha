using com.playbux.events;
using UnityEngine;
using Zenject;

namespace com.playbux
{
    [CreateAssetMenu(menuName = "Playbux/Utilis/RemoteConfig/Create RemoteConfigInstaller", fileName = "RemoteConfigInstaller", order = 0)]
    public class RemoteConfigInstaller : ScriptableObjectInstaller<RemoteConfigInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RemoteConfigProvider>().AsSingle().NonLazy();
            Container.BindSignal<RemoteConfigFetchRequestSignal>().ToMethod<RemoteConfigProvider>(p => p.OnRemoteFetchRequest).FromResolveAll();
        }
    }
}