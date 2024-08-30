using Zenject;
using System.Linq;
using com.playbux.map;
using com.playbux.sorting;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine;

namespace com.playbux.networking.mirror.client.building
{
    public class ClientBuildingController : ILateDisposable
    {
        private readonly IBuilding.Factory factory;
        private readonly IMapController mapController;
        private readonly MapBuildingDatabase database;
        private readonly LayerSorterController sorterController;

        private string currentMapName;
        private Dictionary<int, IBuilding[]> buildings = new Dictionary<int, IBuilding[]>();

        public ClientBuildingController(
            IBuilding.Factory factory,
            IMapController mapController,
            MapBuildingDatabase database,
            LayerSorterController sorterController)
        {
            this.factory = factory;
            this.database = database;
            this.mapController = mapController;
            this.sorterController = sorterController;

            this.mapController.OnCreated += OnMapCreated;
        }

        private void OnMapCreated(string mapName)
        {
            if (mapName == currentMapName)
                return;

            currentMapName = mapName;

            if (string.IsNullOrEmpty(currentMapName))
                return;

            foreach (var pair in buildings)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    sorterController.Remove(pair.Value[i].Sortable);
                    pair.Value[i].Dispose();
                }
            }

            var buildingData = database.Get(currentMapName);
            Assert.IsNotNull(buildingData);

            for (int i = 0; i < buildingData.Length; i++)
            {
                var instance = factory.Create(buildingData[i].prefab, buildingData[i].position);
                instance.Initialize();
                int gridIndex = mapController.PositionToGridIndex(instance.Position);
                var list = buildings.TryGetValue(gridIndex, out var building) ? building.ToList() : new List<IBuilding>();
                list.Add(instance);
                instance.Show();
                sorterController.Add(instance.Sortable);
                buildings[gridIndex] = list.ToArray();
            }
        }

        public void LateDispose()
        {
            mapController.OnCreated -= OnMapCreated;
        }
    }
}