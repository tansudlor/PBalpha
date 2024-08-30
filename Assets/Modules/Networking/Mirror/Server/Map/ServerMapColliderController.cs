using Zenject;
using UnityEngine;
using com.playbux.map;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.collision;

namespace com.playbux.networking.mirror.server.map
{
    public class ServerMapColliderController : ILateDisposable
    {
        private const int RANGE_X = 10;
        private const int RANGE_Y = 10;

        private readonly IMapController mapController;
        private readonly CompositeCollider2DFactory factory;
        private readonly MapColliderDatabase colliderDatabase;
        private readonly IServerMessageSender<MapColliderUpdateMessage> colliderUpdateMessageSender;

        private string currentMap;
        private Dictionary<int, Collider2D> colliders = new Dictionary<int, Collider2D>();

        public ServerMapColliderController(
            CompositeCollider2DFactory factory,
            IMapController mapController,
            MapColliderDatabase colliderDatabase,
            IServerMessageSender<MapColliderUpdateMessage> colliderUpdateMessageSender)
        {
            this.factory = factory;
            this.colliderDatabase = colliderDatabase;
            this.mapController = mapController;
            this.colliderUpdateMessageSender = colliderUpdateMessageSender;

            this.mapController.OnCreated += Initialize;
            this.colliderUpdateMessageSender.Message += Send;
            this.colliderUpdateMessageSender.SendCondition += SendCondition;
        }

        private void Initialize(string name)
        {
            if (currentMap == name)
                return;

            currentMap = colliderDatabase.ContainsKey(name) ? name : "";

            if (string.IsNullOrEmpty(currentMap))
                return;

            var colliderDict = colliderDatabase.Get(currentMap);

            foreach (var pair in colliderDict)
            {
                int index = mapController.PositionToGridIndex(pair.Key);
                var collider = factory.Create(pair.Value);
                colliders.TryAdd(index, collider);
            }
        }

        public void Dispose()
        {
            if (string.IsNullOrEmpty(currentMap))
                return;

            foreach (var pair in colliders)
            {
                Object.Destroy(pair.Value);
            }

            colliders.Clear();
            currentMap = "";
        }

        public void LateDispose()
        {
            Dispose();
            mapController.OnCreated -= Initialize;
            colliderUpdateMessageSender.Message -= Send;
            colliderUpdateMessageSender.SendCondition -= SendCondition;
        }

        private bool SendCondition()
        {
            return !string.IsNullOrEmpty(currentMap);
        }

        private MapColliderUpdateMessage Send()
        {
            return new MapColliderUpdateMessage(currentMap);
        }

        [CanBeNull]
        public Collider2D GetCollider(Vector2 position)
        {
            int centerIndex = mapController.PositionToGridIndex(position);
            int[] vertices = mapController.GetVariableSizeIndicesAroundCenter(centerIndex, RANGE_X, RANGE_Y);
            int closeistIndex = -1;
            float distance = float.PositiveInfinity;

            for (int i = 0; i < vertices.Length; i++)
            {
                float delta = Vector2.Distance(colliders[i].transform.position, position);

                if (delta >= distance)
                    continue;

                distance = delta;
                closeistIndex = i;
            }

            return closeistIndex < 0 ? null : colliders[vertices[closeistIndex]];
        }
    }
}