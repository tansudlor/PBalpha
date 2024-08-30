using System;
using System.Linq;
using UnityEngine;
using JetBrains.Annotations;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.mirror.client.action
{
    [Serializable]
    [CreateAssetMenu(menuName = "Playbux/Action/Create ClientActionDatabase", fileName = "ClientActionDatabase", order = 0)]
    public class ClientActionDatabase : ScriptableObject
    {
        public uint[] Ids => actionAreas.Keys.ToArray();
        public ClientActionAreaData[] Areas => actionAreas.Values.ToArray();

        [SerializeField]
        private SerializedDictionary<uint, ClientActionAreaData> actionAreas = new SerializedDictionary<uint, ClientActionAreaData>();

        [CanBeNull]
        public ClientActionAreaData Get(uint id)
        {
            return actionAreas.TryGetValue(id, out ClientActionAreaData area) ? area : null;
        }
    }
}