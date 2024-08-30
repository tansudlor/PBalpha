using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.playbux.map;
using com.playbux.networking.mirror.message;
using UnityEngine.Assertions;

namespace com.playbux.networking.mirror.server.map
{
    public class ServerMapController : IMapController
    {
        public event Action<string> OnCreated;
        public int Width => mapTotalWidth;
        public int Height => mapTotalHeight;
        public float CartesianWidth => size.x;
        public float CartesianHeight => size.y;
        public Vector2 Offset => offset;

        private const int Y_SCALE = 2;
        private const float CENTER_MULTIPLIER = 0.5f;
        private const float UNITY_METER_SCALE_MULTIPLIER = 0.01f;

        private readonly MapDatabase database;

        private int mapTotalWidth;
        private int mapTotalHeight;
        private int mapChuckWidth;
        private int mapChuckHeight;
        private Vector2 size;
        private Vector2 offset;
        private Map currentMap;
        private Vector2[] grids;
        private List<int> indices;
        private IServerMessageSender<MapDataMessage> mapDataMessageSender;

        public ServerMapController(MapDatabase database, IServerMessageSender<MapDataMessage> mapDataMessageSender)
        {
            this.database = database;
            this.mapDataMessageSender = mapDataMessageSender;
        }

        public void Initialize()
        {
            mapDataMessageSender.Message += Send;
            mapDataMessageSender.SendCondition += SendCondition;

            currentMap = database.Maps.First(map => map.name == "MainLand");
            Assert.IsNotNull(currentMap);
            Create();
        }

        public void Dispose()
        {
            currentMap = null;
            mapDataMessageSender.Message -= Send;
            mapDataMessageSender.SendCondition -= SendCondition;
        }

        private void Create()
        {
            mapTotalWidth = 0;
            mapTotalHeight = 0;

            size = GetMapCartesianSize();
            int mapTotalSize = mapTotalWidth * mapTotalHeight;
            mapChuckWidth = mapTotalWidth / currentMap.width;
            mapChuckHeight = mapTotalHeight / currentMap.height;

            grids = new Vector2[mapTotalSize];
            float offsetX = size.x * CENTER_MULTIPLIER + currentMap.offsetX;
            float offsetY = size.y * CENTER_MULTIPLIER + currentMap.offsetY;

            //NOTE: divide by 4 times beecause it take center of the map and center of the chuck to find a corner
            offset = new Vector2(offsetX, offsetY);
            CalculateGridPosition(mapTotalSize);
            mapDataMessageSender.Send(new MapDataMessage(currentMap.name));
            OnCreated?.Invoke(currentMap.name);
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

        public int PositionToGridIndex(Vector2 position)
        {
            position -= offset;

            // Convert world position to grid coordinates
            int cellX = Mathf.FloorToInt(-position.x / (currentMap.chucks[0].GridSize * UNITY_METER_SCALE_MULTIPLIER));
            int cellY = Mathf.FloorToInt(-position.y / (currentMap.chucks[0].GridSize * UNITY_METER_SCALE_MULTIPLIER * Y_SCALE));

            if (cellX < 0 || cellX > mapTotalWidth || cellY < 0 || cellY > mapTotalHeight)
                return -1;

            return cellY * mapTotalWidth + cellX;
        }

        public int[] GetVariableSizeIndicesAroundCenter(int centerIndex, int gridWidth, int gridHeight)
        {
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

        private bool SendCondition()
        {
            return currentMap != null;
        }

        private MapDataMessage Send()
        {
            return new MapDataMessage(currentMap.name);
        }
    }
}