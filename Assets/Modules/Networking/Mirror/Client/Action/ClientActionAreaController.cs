using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.map;
using com.playbux.action;
using com.playbux.ui.gamemenu;
using System.Collections.Generic;

namespace com.playbux.networking.mirror.client.action
{
    public class ClientActionAreaController : ILateTickable, ILateDisposable
    {
        private readonly IActionArea.Factory factory;
        private readonly IMapController mapController;
        private readonly ClientActionDatabase database;
        private readonly DialogTempleteController dialogTempleteController;
        private readonly Dictionary<int, Dictionary<uint, IActionArea>> actionAreas;

        private bool isOpened;
        private int lastUUID = -1;
        private Dictionary<uint, IActionArea> selectedAreas;

        public ClientActionAreaController(
            IActionArea.Factory factory,
            IMapController mapController,
            ClientActionDatabase database,
            DialogTempleteController entranceBulidingUIController)
        {
            this.factory = factory;
            this.database = database;
            this.mapController = mapController;
            selectedAreas = new Dictionary<uint, IActionArea>();
            this.dialogTempleteController = entranceBulidingUIController;
            actionAreas = new Dictionary<int, Dictionary<uint, IActionArea>>();
            this.dialogTempleteController.OnClose += OnCloseDialog;
            this.mapController.OnCreated += Initialize;
        }

        private void Initialize(string mapName)
        {
            var ids = database.Ids;
            var areas = database.Areas;

            for (int i = 0; i < areas.Length; i++)
            {
                var area = factory.Create(areas[i].position, areas[i].actionArea);
                int gridIndex = mapController.PositionToGridIndex(areas[i].position);
                area.Initialize();

                if (!actionAreas.ContainsKey(gridIndex))
                    actionAreas.Add(gridIndex, new Dictionary<uint, IActionArea>());

                actionAreas[gridIndex][ids[i]] = area;
            }
        }

        public void LateDispose()
        {
            dialogTempleteController.OnClose -= OnCloseDialog;

            foreach (var parentPair in actionAreas)
            {
                foreach (var pair in parentPair.Value)
                {
                    pair.Value.Dispose();
                }
            }

            actionAreas.Clear();
        }

        public void LateTick()
        {
            if (isOpened)
                return;

            if (NetworkClient.localPlayer is null)
                return;

            var position = NetworkClient.localPlayer.transform.position;
            int gridIndex = mapController.PositionToGridIndex(position);
            int[] gridIndices = mapController.GetVariableSizeIndicesAroundCenter(gridIndex, 5, 5);

            if (gridIndices.Length <= 0)
                return;

            selectedAreas.Clear();

            for (int i = 0; i < gridIndices.Length; i++)
            {
                if (!actionAreas.ContainsKey(gridIndices[i]))
                    continue;

                foreach (var pair in actionAreas[gridIndices[i]])
                {
                    selectedAreas.Add(pair.Key, pair.Value);
                }
            }

            if (selectedAreas is { Count: <= 0 })
                return;

            float distance = float.PositiveInfinity;
            int selectedIndex = -1;

            foreach (var pair in selectedAreas)
            {
                float delta = Vector2.Distance(NetworkClient.localPlayer.transform.position, selectedAreas[pair.Key].transform.position);

                if (delta >= distance)
                    continue;

                distance = delta;
                selectedIndex = (int)pair.Key;
            }

            if (selectedIndex < 0)
                return;

            var area = selectedAreas[(uint)selectedIndex];

            bool isIn = area.Validate(NetworkClient.localPlayer.transform.position);

            if (!isIn)
            {
                lastUUID = -1;
                return;
            }

            if (area.UUID == lastUUID)
                return;

            lastUUID = area.UUID;
            area.OnAreaEnter();
        }

        private void OnCloseDialog()
        {
            isOpened = false;
        }
    }
}