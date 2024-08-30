using Zenject;
using UnityEngine;
using com.playbux.events;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.network.api.rest
{
    [CreateAssetMenu(menuName = "Playbux/API/Create RestApiInstaller", fileName = "RestApiInstaller", order = 0)]
    public class RestApiInstaller : ScriptableObjectInstaller<RestApiInstaller>
    {
        [SerializeField]
        private GameObject packedDomainApiInstaller;
        
        [SerializeField]
        private SerializedDictionary<DomainEnum, APISettings> settings;

        public override void InstallBindings()
        {
            BindApiDomains();
            Container.BindInterfacesAndSelfTo<APICredential>().AsSingle();
            Container.Bind<IRESTWorker>().To<RESTWorker>().AsSingle();
            Container.BindInterfacesAndSelfTo<JsonRequest>().AsSingle();
            Container.Bind<PackApiFacade>().FromSubContainerResolve().ByNewContextPrefab(packedDomainApiInstaller).AsSingle().NonLazy();
        }

        private void BindApiDomains()
        {
            var runtimeDict = new Dictionary<DomainEnum, APISettings>();

            foreach (var pair in settings)
            {
                runtimeDict.Add(pair.Key, pair.Value);
            }

            Container.Bind<Dictionary<DomainEnum, APISettings>>().FromInstance(runtimeDict).AsSingle();
        }
    }
}