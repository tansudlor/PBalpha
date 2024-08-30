using System;
using Mirror;
using Zenject;
using UnityEngine;
using System.Linq;
using com.playbux.map;
using System.Collections.Generic;
using com.playbux.networking.mirror.message;
using Cysharp.Threading.Tasks;

namespace com.playbux.networking.mirror.client.map
{
    public class ClientMapController : IInitializable, ILateDisposable, ILateTickable, IMapController
    {
        public event Action<string> OnCreated;
        public int Width => mapTotalWidth;
        public int Height => mapTotalHeight;
        public float CartesianWidth => size.x;
        public float CartesianHeight => size.y;
        public Vector2 Offset => offset;

        private const int Y_SCALE = 2;
        private const int RANGE_X = 12;
        private const int RANGE_Y = 7;
        private const float CENTER_MULTIPLIER = 0.125f;
        private const float UNITY_METER_SCALE_MULTIPLIER = 0.01f;

        private readonly MapDatabase database;
        private readonly CellProvider cellProvider;
        private readonly INetworkMessageReceiver<MapDataMessage> mapDataMessageReceiver;

        private bool isCreating;
        private int mapTotalWidth;
        private int mapTotalHeight;
        private int mapChuckWidth;
        private int mapChuckHeight;
        private Vector2 size;
        private Vector2 offset;
        private Map currentMap;
        private Transform target;
        private Chuck[] chucks;
        private Vector2[] grids;
        private List<int> indices;
        private SpriteRenderer[] spriteRenderers;

        private Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
        private Dictionary<int, RuntimeCell> disposeCells = new Dictionary<int, RuntimeCell>();
        private Dictionary<int, RuntimeCell> runtimeCells = new Dictionary<int, RuntimeCell>();

        public ClientMapController(MapDatabase database, CellProvider cellProvider, INetworkMessageReceiver<MapDataMessage> mapDataMessageReceiver)
        {
            this.database = database;
            this.cellProvider = cellProvider;
            this.mapDataMessageReceiver = mapDataMessageReceiver;
        }

        public void Initialize()
        {
            mapDataMessageReceiver.OnEventCalled += OnMapDataMessageReceived;
        }

        public void LateTick()
        {
            if (isCreating)
                return;

            if (target == null)
                return;

            if (currentMap == null)
                return;

            int index = PositionToGridIndex(target.position + Vector3.left * 3);
            Render(GetVariableSizeIndicesAroundCenter(index, RANGE_X, RANGE_Y));
        }

        public void Dispose()
        {

        }

        public void LateDispose()
        {
            mapDataMessageReceiver.OnEventCalled -= OnMapDataMessageReceived;
        }

        private void OnMapDataMessageReceived(MapDataMessage message)
        {
            WaitForLocalPlayer(message).Forget();
        }

        private async UniTask WaitForLocalPlayer(MapDataMessage message)
        {
            await UniTask.WaitUntil(() => NetworkClient.localPlayer != null);

            if (currentMap is not null)
                return;

            currentMap = database.Maps.FirstOrDefault(map => map.name == message.Name);

            if (currentMap is null)
                return;

            Create();
            OnCreated?.Invoke(message.Name);
            target = NetworkClient.localPlayer.transform;
        }


        public int[] GetVariableSizeIndicesAroundCenter(int centerIndex, int gridWidth, int gridHeight)
        {
            //FIXME: mapTotalWidth Divide by ZERO

            if(mapTotalWidth == 0)
            {
                mapTotalWidth = 1;
                centerIndex = 0;
            }

            int centerX = centerIndex % mapTotalWidth;
            int centerY = centerIndex / mapTotalWidth;
            indices ??= new List<int>();
            indices.Clear();

            // Define the range for the square area
            int startX = centerX - gridWidth / 2;
            int endX = centerX + gridWidth / 2;
            int startY = centerY - gridHeight / 2;
            int endY = centerY + gridHeight / 2;

            // Define starting points for negative and positive hypothetical indexing
            int negativeStartIndex = -1; // Starting point for negative indices
            int positiveStartIndex = mapTotalWidth * mapTotalHeight; // Starting point for positive hypothetical indices

            for (int x = startX; x <= endX; x++)
            {
                for (int y = startY; y <= endY; y++)
                {
                    if (x >= 0 && x < mapTotalWidth && y >= 0 && y < mapTotalHeight)
                    {
                        // Inside the grid
                        indices.Add(y * mapTotalWidth + x);
                    }
                    else
                    {
                        // Outside the grid - calculate a hypothetical index
                        if (x < 0 || y < 0 || x >= mapTotalWidth || y >= mapTotalHeight)
                        {
                            if (x < 0 || y < 0)
                            {
                                // For negative out-of-bounds, decrement from negativeStartIndex
                                indices.Add(negativeStartIndex--);
                            }
                            else
                            {
                                // For positive out-of-bounds, increment from positiveStartIndex
                                indices.Add(positiveStartIndex++);
                            }
                        }
                    }
                }
            }

            return indices.ToArray();
        }

        private void Create()
        {
            isCreating = true;
            mapTotalWidth = 0;
            mapTotalHeight = 0;

            size = GetMapCartesianSize();
            int mapTotalSize = mapTotalWidth * mapTotalHeight;
            mapChuckWidth = mapTotalWidth / currentMap.width;
            mapChuckHeight = mapTotalHeight / currentMap.height;

            grids = new Vector2[mapTotalSize];
            float offsetX = size.x * CENTER_MULTIPLIER + currentMap.offsetX;
            float offsetY = size.y * CENTER_MULTIPLIER + currentMap.offsetY;
            // float offsetY = 0f;

            //NOTE: divide by 4 times beecause it take center of the map and center of the chuck to find a corner
            offset = new Vector2(offsetX, offsetY);
            CalculateGridPosition(mapTotalSize);
            isCreating = false;
        }

        public int PositionToGridIndex(Vector2 position)
        {
            position -= offset;

            // Convert world position to grid coordinates
            int cellX = Mathf.FloorToInt(-position.x / (1024 * UNITY_METER_SCALE_MULTIPLIER));
            int cellY = Mathf.FloorToInt(-position.y / ((1024 * UNITY_METER_SCALE_MULTIPLIER) * Y_SCALE));

            // if (cellX < 0 || cellX > mapTotalWidth || cellY < 0 || cellY > mapTotalHeight)
            //     return -1;

            return cellY * mapTotalWidth + cellX;
        }

        private void CalculateGridPosition(int mapSize)
        {
            for (int i = 0; i < mapSize; i++)
            {
                int chunkIndex = FindChuckIndex(i);

                int cellX = i % mapTotalWidth;
                int cellY = i / mapTotalWidth;

                float worldX = cellX * currentMap.chucks[chunkIndex].GridSize * UNITY_METER_SCALE_MULTIPLIER;
                float worldY = -cellY * currentMap.chucks[chunkIndex].GridSize * UNITY_METER_SCALE_MULTIPLIER; // Negative because Unity's Y axis goes up

                // Adjust starting position based on the cell size and starting position.
                // If you want the center of the cell, then you would add half the cell's width and height.s
                // Otherwise, remove these offsets if you want the top-left corner of the cell.
                worldX += currentMap.chucks[chunkIndex].GridSize * UNITY_METER_SCALE_MULTIPLIER * 0.5f;
                worldY -= currentMap.chucks[chunkIndex].GridSize * UNITY_METER_SCALE_MULTIPLIER * 0.5f; // Negative to go down in Unity's coordinate system

                Vector2 worldPos = new Vector2(-worldX, worldY * Y_SCALE);
                grids[i] = offset + worldPos;
                int localCellNumber = FindLocalIndex(i);

                if (!currentMap.chucks[chunkIndex].Textures.ContainsKey(localCellNumber))
                    continue;

                var texture = currentMap.chucks[chunkIndex].Textures[localCellNumber].HighQuality;
                sprites.TryAdd(i, CreateSprite(texture));
            }
        }

        private Vector2 GetMapCartesianSize()
        {
            size = Vector2.zero;

            for (int i = 0; i < currentMap.chucks.Length; i++)
            {
                if (i < currentMap.width)
                    mapTotalWidth += currentMap.chucks[i].Width;

                if (i < currentMap.height)
                    mapTotalHeight += currentMap.chucks[i].Height;

                size.x += currentMap.chucks[i].Width * currentMap.chucks[i].GridSize * UNITY_METER_SCALE_MULTIPLIER;
                size.y += currentMap.chucks[i].Height * currentMap.chucks[i].GridSize * UNITY_METER_SCALE_MULTIPLIER;
            }

            return size;
        }

        private void Render(params int[] indexs)
        {
            if (indexs.Length <= 0)
                return;

            Cull(indexs);

            for (int i = 0; i < indexs.Length; i++)
                Render(indexs[i]);
        }

        private void Render(int index)
        {
            if (index < 0)
                return;

            if (!sprites.ContainsKey(index))
                return;

            var cell = cellProvider.Get(grids[index], sprites[index]);
#if UNITY_EDITOR
            cell.GameObject.name = $"Cell#{index}";
#endif
            cell.Transform.localScale = new Vector3(-1, Y_SCALE, 1);
            runtimeCells.TryAdd(index, cell);
        }

        private Sprite CreateSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
        }

        private int FindChuckIndex(int index)
        {
            int cellX = index % mapTotalWidth;
            int cellY = index / mapTotalWidth;

            int chunkX = cellX / mapChuckWidth;
            int chunkY = cellY / mapChuckHeight;

            return chunkY * currentMap.width + chunkX;
        }

        private int FindLocalIndex(int index)
        {
            int cellX = index % mapTotalWidth;
            int cellY = index / mapTotalWidth;

            int chunkX = cellX / mapChuckWidth;
            int chunkY = cellY / mapChuckHeight;

            int localCellX = cellX - chunkX * mapChuckWidth;
            int localCellY = cellY - chunkY * mapChuckHeight;

            // Calculate local cell number within chuck
            return (localCellY * mapChuckWidth + localCellX) + 1; //NOTE: +1 because texture index from PS start from 1 instead of 0
        }

        private void Cull(params int[] indexs)
        {
            foreach (var pair in runtimeCells)
            {
                for (int i = 0; i < indexs.Length; i++)
                {
                    if (pair.Key == indexs[i])
                        continue;

                    disposeCells.TryAdd(pair.Key, pair.Value);
                }
            }

            foreach (var pair in disposeCells)
            {
                runtimeCells.Remove(pair.Key);
                cellProvider.Return(pair.Value);
            }

            disposeCells.Clear();
        }
    }
}