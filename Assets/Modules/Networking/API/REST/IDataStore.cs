using Zenject;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace com.playbux.network.api.rest
{
    public interface IDataStore
    {
        DomainEnum DomainEnum { get; }
        UniTask<RestMessage<T>> Get<T>(NameValueCollection body, string segments);
        UniTask<RestMessage<T>> Post<T>(object body, string segments);
        UniTask<RestMessage<T>> Post<T>(NameValueCollection query, object body, string segments);

        public class Factory : PlaceholderFactory<PackApiFacade>
        {
            
        }
    }
}