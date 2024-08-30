using Zenject;
using UnityEngine;
using com.playbux.map;
using System.Collections.Generic;
using com.playbux.networking.mirror.collision;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.client.map
{
    public class ClientMapColliderController : IInitializable, ILateDisposable
    {
        private readonly CompositeCollider2DFactory factory;
        private readonly IMapController mapController;
        private readonly MapColliderDatabase database;
        private readonly INetworkMessageReceiver<MapColliderUpdateMessage> colliderUpdateMessageReceiver;

        private Dictionary<int, Collider2D> colliders = new Dictionary<int, Collider2D>();

        public ClientMapColliderController(
            IMapController mapController,
            MapColliderDatabase database,
            CompositeCollider2DFactory factory,
            INetworkMessageReceiver<MapColliderUpdateMessage> colliderUpdateMessageReceiver)
        {
            this.factory = factory;
            this.database = database;
            this.mapController = mapController;
            this.colliderUpdateMessageReceiver = colliderUpdateMessageReceiver;
        }

        public void Initialize()
        {
            colliderUpdateMessageReceiver.OnEventCalled += OnMessageReceived;
        }
        public void LateDispose()
        {
            colliderUpdateMessageReceiver.OnEventCalled -= OnMessageReceived;
        }

        public void OnMessageReceived(MapColliderUpdateMessage message)
        {
            if (!database.ContainsKey(message.Name))
                return;

            var currentMap = database.Get(message.Name);

            foreach (var pair in currentMap)
            {
                int key = mapController.PositionToGridIndex(pair.Key);

                if (colliders.ContainsKey(key))
                {
                    if ((Vector2)colliders[key].transform.position == pair.Key)
                        continue;

                    Object.Destroy(colliders[key].gameObject);
                    colliders.Remove(key);
                }

                var collider = factory.Create(pair.Value);
                colliders.TryAdd(key, collider);
            }
        }
    }
}