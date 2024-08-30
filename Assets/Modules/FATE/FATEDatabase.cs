using UnityEngine;
using System.Linq;

namespace com.playbux.FATE
{

    [CreateAssetMenu(menuName = "Playbux/FATE/Create FATEDatabase", fileName = "FATEDatabase", order = 0)]
    public class FATEDatabase : ScriptableObject
    {
        public FATEData[] Data => data;

        public int Count => data.Length;

        [SerializeField]
        private FATEData[] data;
        
        public FATEData? Get(uint id)
        {
            FATEData? data;
            data = this.data.FirstOrDefault(fate => fate.id == id);
            return data;
        }

#if UNITY_EDITOR
        public void Add(FATEData data)
        {
            var hashData = this.data.ToHashSet();
            hashData.Add(data);
            this.data = hashData.ToArray();
        }

        public void Remove(FATEData data)
        {
            var hashData = this.data.ToHashSet();
            hashData.Remove(data);
            this.data = hashData.ToArray();
        }
#endif
    }
}