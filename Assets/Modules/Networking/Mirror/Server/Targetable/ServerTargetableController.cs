using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace com.playbux.networking.server.targetable
{
    public class ServerTargetableController
    {
        public bool IsTargetable(uint netId)
        {
            bool hasTarget = false;

            foreach(var pair in targetables)
            {
                var list = targetables[pair.Key].ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[0] != netId)
                        continue;

                    hasTarget = true;
                }
            }

            return hasTarget;
        }

        private readonly Dictionary<int, uint[]> targetables = new Dictionary<int, uint[]>();

        public void Add(uint netId, Vector3 position)
        {
            var positionIndexes = NetworkServer.aoi.GetPosition(position);
            for (int i = 0; i < positionIndexes.Length; i++)
            {
                var netIdList = targetables.TryGetValue(positionIndexes[i], out uint[] targetable) ? targetable.ToHashSet() : new HashSet<uint>();
                netIdList.Add(netId);

                if (targetables.ContainsKey(positionIndexes[i]))
                {
                    targetables[positionIndexes[i]] = netIdList.ToArray();
                    return;
                }

                targetables.Add(positionIndexes[i], netIdList.ToArray());
            }
        }

        public void Remove(uint netId)
        {
            foreach(var pair in targetables)
            {
                var list = targetables[pair.Key].ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[0] != netId)
                        continue;

                    list.Remove(netId);
                }
            }
        }
    }
}