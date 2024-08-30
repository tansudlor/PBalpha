using System;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.FATE
{
    [CreateAssetMenu(menuName = "Playbux/FATE/Create FATEPositionDatabase", fileName = "FATEPositionDatabase", order = 0)]
    public class FATEPositionDatabase : ScriptableObject
    {
        public int Count => fatePositions.Count;

        [SerializeField]
        private SerializedDictionary<FATEPositionKey, Vector2[]> fatePositions = new SerializedDictionary<FATEPositionKey, Vector2[]>();

#if UNITY_EDITOR
        public void Add(FATEPositionKey key, Vector2 position)
        {
            if (!fatePositions.ContainsKey(key))
                fatePositions.Add(key, Array.Empty<Vector2>());

            var hashPosition = fatePositions[key] != null ? fatePositions[key].ToHashSet() : new HashSet<Vector2>();
            hashPosition.Add(position);
            fatePositions[key] = hashPosition.ToArray();
        }

        public void Add(FATEPositionKey key, Vector2[] positions)
        {
            if (!fatePositions.ContainsKey(key))
                fatePositions.Add(key, Array.Empty<Vector2>());

            var hashPosition = fatePositions[key] != null ? fatePositions[key].ToHashSet() : new HashSet<Vector2>();
            for (int i = 0; i < positions.Length; i++)
                hashPosition.Add(positions[i]);

            fatePositions[key] = hashPosition.ToArray();
        }

        [CanBeNull]
        public Vector2[] Get(FATEPositionKey key)
        {
            if (!fatePositions.ContainsKey(key))
                return null;

            return fatePositions[key].Length <= 0 ? null : fatePositions[key];
        }

        public void Remove(FATEPositionKey key)
        {
            if (!fatePositions.ContainsKey(key))
                return;

            fatePositions.Remove(key);
        }

        public void Remove(FATEPositionKey key, Vector2 position)
        {
            if (!fatePositions.ContainsKey(key))
                return;

            var hashPosition = fatePositions[key] != null ? fatePositions[key].ToHashSet() : new HashSet<Vector2>();

            foreach (var pos in fatePositions[key])
            {
                if (pos != position)
                    continue;

                hashPosition.Remove(pos);
            }

            fatePositions[key] = hashPosition.ToArray();
        }
#endif
    }
}