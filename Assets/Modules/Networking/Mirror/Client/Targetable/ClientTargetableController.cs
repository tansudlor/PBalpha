using Mirror;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace com.playbux.networking.client.targetable
{
    public class ClientTargetableController
    {
        public bool IsTargetable(int gridIndex, uint netId)
        {
            bool hasTarget = false;

            var list = targetables[gridIndex].ToList();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != netId)
                    continue;

                hasTarget = true;
            }

            return hasTarget;
        }

        public NetworkIdentity Target(uint ownNetId, uint? targetedNetId, int[] gridIndexs)
        {
            try //FIXME: KeyNotFoundException: The given key '2' was not present in the dictionary.
            {
                for (int i = 0; i < gridIndexs.Length; i++)
                {
                    var list = targetables[gridIndexs[i]].ToList();

                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j] == ownNetId)
                            continue;

                        if (targetedNetId.HasValue && list[j] != targetedNetId)
                            continue;

                        return NetworkClient.spawned[list[j]];
                    }
                }
            }
            catch (Exception E)
            { 

            }

            return null;
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