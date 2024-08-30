using System.Linq;
using System.Collections.Generic;


namespace com.playbux.servercollection
{
    public abstract class ServerCollection<TKey, TValue>
    {
        // Start is called before the first frame update
        protected Dictionary<TKey, TValue> datas = new Dictionary<TKey, TValue>();

        public void Assign(TKey key, TValue state)
        {

            datas[key] = state;
        }

        public void Remove(TKey key)
        {
            datas.Remove(key);
        }

        public abstract TValue GetState(TKey key);

        public abstract void Clean();

        public TKey[] GetCollection()
        {
            return datas.Keys.ToArray();
        }

    }
}
