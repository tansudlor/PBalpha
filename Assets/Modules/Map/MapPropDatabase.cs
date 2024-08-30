using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.map
{
    [CreateAssetMenu(menuName = "Playbux/Map/Create MapPropDatabase", fileName = "MapPropDatabase")]
    public class MapPropDatabase : ScriptableObject
    {
        public string[] Keys => props.Keys.ToArray();
        public PropDataCollection[] Data => props.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<string, PropDataCollection> props = new SerializedDictionary<string, PropDataCollection>();

#if UNITY_EDITOR
        public void Add(string mapKey, PropDataWrapper propDataWrapper)
        {
            List<PropDataWrapper> list;
            PropDataCollection collection = new PropDataCollection();
            if (!props.ContainsKey(mapKey))
            {
                list = new List<PropDataWrapper>();
                list.Add(propDataWrapper);
                collection.data = list.ToArray();
                props.Add(mapKey, collection);
                return;
            }

            list = props[mapKey].data.ToList();
            list.Add(propDataWrapper);
            collection.data = list.ToArray();
            props[mapKey] = collection;
        }
#endif

        public bool ContainsKey(string name) => props.ContainsKey(name);

        public PropDataWrapper[] Get(string name)
        {
            if (!props.ContainsKey(name))
                return null;

            return props[name].data;
        }
    }
}