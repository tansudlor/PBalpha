using System;
using Zenject;
using UnityEngine;
using com.playbux.map;
using System.Collections.Generic;
using com.playbux.networking.mirror.collision;
using com.playbux.networking.mirror.message;
using Object = UnityEngine.Object;

namespace com.playbux.networking.mirror.server.prop
{
    public class ServerPropController : ILateDisposable
    {
        private const int RANGE_X = 10;
        private const int RANGE_Y = 10;

        private readonly PropDatabase propDatabase;
        private readonly IMapController mapController;
        private readonly MapPropDatabase mapPropDatabase;
        private readonly Collider2DFactory colliderFactory;
        private readonly IServerMessageSender<PropColliderUpdateMessage> colliderUpdateMessageSender;

        private string currentMap;
        private string[] names;
        private int[] positions;
        private Vector3[] scales;
        private Quaternion[] rotations;
        private Dictionary<int, Collider2D> colliders = new Dictionary<int, Collider2D>();

        public ServerPropController(
            PropDatabase propDatabase,
            IMapController mapController,
            MapPropDatabase mapPropDatabase,
            Collider2DFactory colliderFactory,
            IServerMessageSender<PropColliderUpdateMessage> colliderUpdateMessageSender)
        {
            this.propDatabase = propDatabase;
            this.mapController = mapController;
            this.mapPropDatabase = mapPropDatabase;
            this.colliderFactory = colliderFactory;
            this.colliderUpdateMessageSender = colliderUpdateMessageSender;

            this.mapController.OnCreated += Initialize;
            this.colliderUpdateMessageSender.Message += SendColliderUpdate;
            this.colliderUpdateMessageSender.SendCondition += SendColliderUpdateCondition;
        }

        private void Initialize(string name)
        {
            if (currentMap == name)
                return;

            currentMap = mapPropDatabase.ContainsKey(name) ? name : "";

            if (string.IsNullOrEmpty(currentMap))
                return;

            var data = mapPropDatabase.Get(name);
            names = new string[data.Length];
            positions = new int[data.Length];
            scales = new Vector3[data.Length];
            rotations = new Quaternion[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                var propData = propDatabase.Get(data[i].name);

                if (!propData.HasValue)
                    continue;

                positions[i] = 0;
                names[i] = data[i].name;
                scales[i] = data[i].scale;
                rotations[i] = propData.Value.propObject.transform.rotation;

                if (data[i].flip)
                    rotations[i].z *= -1;

                for (int j = 0; j < propData.Value.propCollider.Length; j++)
                {
                    if (propData.Value.propCollider[j] == null)
                        continue;

                    var rotation = propData.Value.propCollider[j].transform.rotation;

                    if (data[i].flip)
                        rotation.z *= -1;

                    var collider = colliderFactory.Create(propData.Value.propCollider[j]);
                    int index = mapController.PositionToGridIndex(data[i].position);
                    collider.transform.position = (Vector2)propData.Value.propCollider[j].transform.position * data[i].scale + data[i].position;
                    collider.transform.rotation = rotation;
                    collider.transform.localScale = data[i].scale;
                    positions[i] = positions[i] != 0 ? HashCode.Combine(positions[i], collider.transform.position.GetHashCode()) : collider.transform.position.GetHashCode();
                    colliders.TryAdd(index, collider);
                }
            }
        }

        public void LateDispose()
        {
            mapController.OnCreated -= Initialize;
            colliderUpdateMessageSender.Message -= SendColliderUpdate;
            colliderUpdateMessageSender.SendCondition -= SendColliderUpdateCondition;

            if (string.IsNullOrEmpty(currentMap))
                return;

            foreach (var pair in colliders)
            {
                Object.Destroy(pair.Value);
            }

            colliders.Clear();
            currentMap = "";
        }

        private bool SendColliderUpdateCondition()
        {
            return !string.IsNullOrEmpty(currentMap) && names is { Length: > 0 } && positions is { Length: > 0 } && scales is { Length: > 0 };
        }

        private PropColliderUpdateMessage SendColliderUpdate()
        {
            return new PropColliderUpdateMessage(names, positions, rotations, scales);
        }
    }
}