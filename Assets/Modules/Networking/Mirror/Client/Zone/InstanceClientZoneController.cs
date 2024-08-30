using Zenject;
using com.playbux;
using UnityEngine;
using com.playbux.zone;

namespace om.playbux.networking.mirror.client.zone
{
    public class InstanceClientZoneController : IInitializable, ILateDisposable
    {
        private readonly ZoneDatabase database;
        private readonly PrefabFactory prefabFactory;
        private readonly CompositeCollider2DFactory colliderFactory;

        private Collider2D[] colliders;
        public InstanceClientZoneController(ZoneDatabase database, PrefabFactory prefabFactory, CompositeCollider2DFactory colliderFactory)
        {
            this.database = database;
            this.prefabFactory = prefabFactory;
            this.colliderFactory = colliderFactory;
        }

        public void Initialize()
        {
            colliders = new Collider2D[database.Keys.Length];

            for (int i = 0; i < database.Keys.Length; i++)
            {
                var asset = database.Get(database.Keys[i]);

                if (asset == null)
                    continue;

                var gameObject = prefabFactory.Create(asset.Value.prefab);
                gameObject.transform.position = database.Keys[i].position;
                colliders[i] = colliderFactory.Create(asset.Value.collider);
                colliders[i].transform.position = database.Keys[i].position;
            }
        }

        public void LateDispose()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Object.Destroy(colliders[i]);
            }
        }
    }
}