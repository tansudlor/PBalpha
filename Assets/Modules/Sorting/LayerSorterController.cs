using UnityEngine;
using System.Linq;
using com.playbux.map;
using JetBrains.Annotations;
using System.Collections.Generic;
using Zenject;

namespace com.playbux.sorting
{
    public class LayerSorterController : ILateDisposable
    {
        private const int RANGE_X = 10;
        private const int RANGE_Y = 10;

        private readonly IMapController mapController;

        private string currentMap;
        private Dictionary<int, ISortable[]> sortables = new Dictionary<int, ISortable[]>();

        public LayerSorterController(IMapController mapController)
        {
            this.mapController = mapController;
            this.mapController.OnCreated += OnMapCreated;
        }

        private void OnMapCreated(string mapName)
        {
            currentMap = mapName;
        }

        public void LateDispose()
        {
            mapController.OnCreated -= OnMapCreated;
            currentMap = "";
            sortables.Clear();
        }

        public void Add(ISortable sortable)
        {
            if (string.IsNullOrEmpty(currentMap))
                return;

            int gridIndex = mapController.PositionToGridIndex(sortable.Position);
            var tempSortables = sortables.TryGetValue(gridIndex, out var array) ? array.ToList() : new List<ISortable>();
            tempSortables.Add(sortable);
            sortables[gridIndex] = tempSortables.ToArray();
        }

        public void Remove(ISortable sortable)
        {
            int gridIndex = mapController.PositionToGridIndex(sortable.Position);

            if (!sortables.ContainsKey(gridIndex))
                return;

            var tempSortables = sortables[gridIndex].ToList();
            tempSortables.Remove(sortable);
            sortables[gridIndex] = tempSortables.ToArray();
        }

        [CanBeNull]
        public ISortable[] Get(Vector2 position)
        {
            int index = mapController.PositionToGridIndex(position);
            int[] indices = mapController.GetVariableSizeIndicesAroundCenter(index, RANGE_X, RANGE_Y);

            if (indices.Length <= 0)
                return null;

            var selectedSortables = new List<ISortable>();

            for (int i = 0; i < indices.Length; i++)
            {
                if (!sortables.ContainsKey(indices[i]))
                    continue;

                for (int j = 0; j < sortables[indices[i]].Length; j++)
                {
                    selectedSortables.Add(sortables[indices[i]][j]);
                }
            }

            return selectedSortables.ToArray();
        }
    }
}