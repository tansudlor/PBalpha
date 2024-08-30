using System.Collections.Generic;
using JetBrains.Annotations;

namespace com.playbux.network.api.rest
{
    public class PackApiFacade
    {
        private readonly Dictionary<DomainEnum, IDataStore> domainDataStore;
        
        public PackApiFacade(List<IDataStore> dataStores)
        {
            domainDataStore = new Dictionary<DomainEnum, IDataStore>();
            
            foreach (var dataStore in dataStores)
                domainDataStore.Add(dataStore.DomainEnum, dataStore);
        }

        [CanBeNull]
        public IDataStore GetDataStore(DomainEnum domainEnum)
        {
            return domainDataStore.TryGetValue(domainEnum, out var value) ? value : null;
        }
    }
}