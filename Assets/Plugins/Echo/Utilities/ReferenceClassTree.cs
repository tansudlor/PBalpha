#nullable enable
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace SETHD.Utilities
{
    [Serializable]
    public class ReferenceClassTree<T> : ISerializationCallbackReceiver where T : class
    {
        public int Count => rawData.Length; 
        
        [SerializeField]
        private string separator = "/";

        [SerializeField]
        private SerializedData[] rawData = new SerializedData[0];

        private Dictionary<string, T> database = new Dictionary<string, T>();
        private Dictionary<string, (bool, T)> cached = new Dictionary<string, (bool, T)>();

        public bool TryGet(in string key, out T val)
        {
            if (cached.TryGetValue(key, out var outValue))
            {
                var (ret, value) = outValue;
                val = value;
                return ret;
            }

            var currentKey = key;
            while (!string.IsNullOrEmpty(currentKey))
            {
                if (database.TryGetValue(currentKey, out val))
                {
                    if (val != default(T))
                    {
                        cached.Add(key, (true, val));
                        return true;
                    }
                }
                var lastIndex = currentKey.LastIndexOf(separator, StringComparison.Ordinal);
                if (lastIndex == -1)
                {
                    break;
                }
                currentKey = key.Substring(0, lastIndex);
            }

            cached.Add(key, (false, default!));
            val = default!;
            return false;
        }

        public bool TryGet(in int index, out SerializedData val)
        {
            if (database.Count <= 0)
            {
                val = default!;
                return false;
            }

            val = rawData[index];
            return true;
        }
        
#if UNITY_EDITOR
        public void Edit(string key, T newData, string editKey = "")
        {
            for (int i = 0; i < rawData.Length; i++)
            {
                if (rawData[i].key != key) 
                    continue;
                
                rawData[i].value = newData;

                if (!string.IsNullOrEmpty(editKey) && rawData[i].key != editKey)
                    rawData[i].key = editKey;
                
                break;
            }
        }
        
        public void TryEdit(string key, T newData, string editKey = "")
        {
            var list = rawData.ToList();
            
            if (!list.Exists(data => data.key == key))
            {
                list.Add(new SerializedData{ key = key, value = newData });
                rawData = list.ToArray();
                return;
            }
            
            for (int i = 0; i < rawData.Length; i++)
            {
                if (rawData[i].key != key) 
                    continue;
                
                rawData[i].value = newData;

                if (!string.IsNullOrEmpty(editKey) && rawData[i].key != editKey)
                    rawData[i].key = editKey;
                
                break;
            }
        }

        public SerializedData Read(string keySearch)
        {
            return rawData.Where(data => data.key.Contains(keySearch.ToLower())).Select(data => data).GetEnumerator().Current;
        }
        
        public SerializedData[] ReadMany(string keySearch)
        {
            return rawData.Where(data => data.key.ToLower().Contains(keySearch.ToLower())).ToArray();
        }

        public string[] GetKeys(string keySearch)
        {
            return rawData.Where(data => data.key.ToLower().Contains(keySearch.ToLower())).Select(data => data.key).ToArray();
        }

        public void Add(string key, T val)
        {
            var listData = rawData.ToList();

            if (listData.Exists(data => data.key == key))
                return;
            
            var newData = new SerializedData
            {
                key = key,
                value = val
            };
            
            listData.Add(newData);
            rawData = listData.ToArray();
        }

        public bool Remove(string key)
        {
            var listData = rawData.ToList();

            if (!listData.Exists(data => data.key == key))
                return false;
            
            listData.RemoveAll(data => data.key == key);
            rawData = listData.ToArray();
            return true;
        }
#endif
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            var splitter = new[] { separator };
            foreach (var data in rawData)
            {
                var separated = data.key.Split(splitter, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < separated.Length; ++i)
                {
                    var key = string.Join(separator, separated, 0, i+1);
                    database.TryAdd(key, default!);
                }
                database[data.key] = data.value;
            }
        }

        [Serializable]
        public struct SerializedData
        {
            public string key;
            public T value;
        }
    }
}