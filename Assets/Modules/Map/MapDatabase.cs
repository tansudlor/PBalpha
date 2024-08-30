using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.playbux.map
{
    [CreateAssetMenu(menuName = "Playbux/Map/Create Database", fileName = "MapDatabase")]
    public class MapDatabase : ScriptableObject
    {
        public Map[] Maps => maps;

        [SerializeField]
        private Map[] maps;

#region EDITOR
#if UNITY_EDITOR
        public void CreateMap(string name)
        {
            var temp = new List<Map>();
            var map = new Map
            {
                name = name
            };

            if (maps.Equals(null) || maps.Length <= 0)
            {
                temp.Add(map);
                maps = temp.ToArray();
                return;
            }

            temp = maps.ToList();
            temp.Add(map);
            maps = temp.ToArray();
        }
#endif
#endregion
    }
}