using Zenject;
using UnityEngine;
using com.playbux.events;
using UnityEngine.Assertions;
using System.Collections.Generic;

namespace com.playbux.network.api.rest
{
    public class PlaybuxGameBackendApiInstaller : MonoInstaller<PlaybuxGameBackendApiInstaller>
    {
        [SerializeField]
        private DomainEnum domainEnum;

        [Inject]
        private Dictionary<DomainEnum, APISettings> domainDict;
        
        public override void InstallBindings()
        {
            Assert.IsTrue(domainDict.ContainsKey(domainEnum));
            Container.Bind<UrlHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<RESTDataStore>().AsSingle();
            Container.Bind<APISettings>().FromInstance(domainDict[domainEnum]).AsSingle();

            Container.BindSignal<RemoteConfigResponseSignal<string>>().ToMethod<RESTDataStore>(c => c.OnRemoteConfigDomainResponse).FromResolveAll();
            Container.BindSignal<RemoteConfigResponseSignal<string>>().ToMethod<RESTDataStore>(c => c.OnRemoteConfigApiVersionResponse).FromResolveAll();
        }
    }
}