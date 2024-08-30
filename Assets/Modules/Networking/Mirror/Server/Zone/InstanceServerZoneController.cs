using Zenject;
using com.playbux;
using UnityEngine;
using com.playbux.zone;

namespace om.playbux.networking.mirror.server.zone
{
    public class InstanceServerZoneController : IInitializable, ILateDisposable
    {
        private readonly ZoneDatabase database;
        private readonly CompositeCollider2DFactory factory;

        private Collider2D[] colliders;

        public InstanceServerZoneController(ZoneDatabase database, CompositeCollider2DFactory factory)
        {
            this.database = database;
            this.factory = factory;
        }

        public void Initialize()
        {
            colliders = new Collider2D[database.Keys.Length];

            for (int i = 0; i < database.Keys.Length; i++)
            {
                var asset = database.Get(database.Keys[i]);

                if (asset == null)
                    continue;

                colliders[i] = factory.Create(asset.Value.collider);
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