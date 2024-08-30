using System;
using Mirror;
using Zenject;
using System.Linq;
using UnityEngine;
using com.playbux.map;
using com.playbux.sorting;
using JetBrains.Annotations;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.collision;

namespace com.playbux.networking.mirror.client.prop
{
    public class ClientPropController : ILateDisposable
    {
        private const int RANGE_X = 10;
        private const int RANGE_Y = 10;

        private readonly PropDatabase propDatabase;
        private readonly IMapController mapController;
        private readonly ClientProp.Factory propFactory;
        private readonly MapPropDatabase mapPropDatabase;
        private readonly Collider2DFactory colliderFactory;
        private readonly LayerSorterController layerSorterController;
        private readonly INetworkMessageReceiver<MapDataMessage> mapDataMessageReceiver;
        private readonly INetworkMessageReceiver<PropColliderUpdateMessage> propColliderUpdateMessageReceiver;

        private string currentMap;
        private Dictionary<int, Collider2D> hashDict = new Dictionary<int, Collider2D>();
        private Dictionary<int, ClientProp[]> props = new Dictionary<int, ClientProp[]>();
        private Dictionary<int, Collider2D[]> colliders = new Dictionary<int, Collider2D[]>();

        public ClientPropController(
            PropDatabase propDatabase,
            IMapController mapController,
            ClientProp.Factory propFactory,
            MapPropDatabase mapPropDatabase,
            Collider2DFactory colliderFactory,
            LayerSorterController layerSorterController,
            INetworkMessageReceiver<MapDataMessage> mapDataMessageReceiver,
            INetworkMessageReceiver<PropColliderUpdateMessage> propColliderUpdateMessageReceiver)
        {
            this.propFactory = propFactory;
            this.propDatabase = propDatabase;
            this.mapController = mapController;
            this.mapPropDatabase = mapPropDatabase;
            this.colliderFactory = colliderFactory;
            this.layerSorterController = layerSorterController;
            this.mapDataMessageReceiver = mapDataMessageReceiver;
            this.propColliderUpdateMessageReceiver = propColliderUpdateMessageReceiver;

            this.mapController.OnCreated += Initialize;
            this.mapDataMessageReceiver.OnEventCalled += OnMapDataUpdateMessageReceived;
            this.propColliderUpdateMessageReceiver.OnEventCalled += OnPropColliderUpdateMessageReceived;
        }

        private void Initialize(string name)
        {
            currentMap = mapPropDatabase.ContainsKey(name) ? name : "";

            if (string.IsNullOrEmpty(currentMap))
                return;

            Create();
        }

        public void LateDispose()
        {
            mapController.OnCreated -= Initialize;
            mapDataMessageReceiver.OnEventCalled -= OnMapDataUpdateMessageReceived;
            propColliderUpdateMessageReceiver.OnEventCalled -= OnPropColliderUpdateMessageReceived;

            if (string.IsNullOrEmpty(currentMap))
                return;

            Clear();
        }

        private void Create()
        {
            var data = mapPropDatabase.Get(currentMap);

            if (data == null)
                return;

            for (int i = 0; i < data.Length; i++)
            {
                var propData = propDatabase.Get(data[i].name);

                if (!propData.HasValue)
                    continue;

                var prop = propFactory.Create(propData.Value.propObject, data[i].position, data[i].scale);
                prop.Sortable.Initialize();
                int index = mapController.PositionToGridIndex(data[i].offsetPostion != Vector2.zero ? data[i].position + data[i].offsetPostion : data[i].position);
                var tempProps = props.TryGetValue(index, out var propArray) ? propArray.ToList() : new List<ClientProp>();

                for (int j = 0; j < propData.Value.propCollider.Length; j++)
                {
                    var tempColls = new List<Collider2D>();
                    var rotation = propData.Value.propCollider[j].transform.rotation;

                    if (data[i].flip)
                        rotation.z *= -1;

                    var collider = colliderFactory.Create(propData.Value.propCollider[j]);
                    collider.transform.position = (Vector2)propData.Value.propCollider[j].transform.position * data[i].scale + data[i].position;
                    collider.transform.rotation = rotation;
                    collider.transform.localScale = data[i].scale;

                    int hash = 0;
                    int scaleHash = collider.transform.localScale.GetHashCode();
                    int positionHash = collider.transform.position.GetHashCode();
                    int rotationHash = propData.Value.propObject.transform.rotation.GetHashCode();
                    hash = HashCode.Combine(positionHash, rotationHash, scaleHash);

                    hashDict.TryAdd(hash, collider);

                    if (!colliders.ContainsKey(index))
                    {
                        tempColls.Add(collider);
                        colliders.Add(index, tempColls.ToArray());
                        continue;
                    }

                    tempColls = colliders[index].ToList();
                    tempColls.Add(collider);
                    colliders[index] = tempColls.ToArray();
                }

                prop.name = data[i].name;
                tempProps.Add(prop);

                if (!prop.Sortable.IgnoreSorting)
                {
                    int gridIndex = mapController.PositionToGridIndex(prop.Sortable.Position);
                    layerSorterController.Add(prop.Sortable);
                }

                props[index] = tempProps.ToArray();
            }
        }

        private void Clear()
        {
            foreach (var pair in colliders)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    Object.Destroy(pair.Value[i]);
                }
            }

            foreach (var pair in props)
            {
                for (int i = 0; i < pair.Value.Length; i++)
                {
                    layerSorterController.Remove(pair.Value[i].Sortable);
                    Object.Destroy(pair.Value[i]);
                }
            }

            props.Clear();
            hashDict.Clear();
            colliders.Clear();
            currentMap = "";
        }

        private async UniTask WaitForLocalPlayer(MapDataMessage message)
        {
            await UniTask.WaitUntil(() => NetworkClient.localPlayer != null);

            if (currentMap is not null)
                return;

            currentMap = message.Name;

            if (currentMap is null)
                return;

            Clear();
            Create();
        }

        private void OnMapDataUpdateMessageReceived(MapDataMessage message)
        {
            WaitForLocalPlayer(message).Forget();
        }

        private void OnPropColliderUpdateMessageReceived(PropColliderUpdateMessage message)
        {
            if (string.IsNullOrEmpty(currentMap))
                return;

            var data = mapPropDatabase.Get(currentMap);

            for (int i = 0; i < message.Names.Length; i++)
            {
                int index = mapController.PositionToGridIndex(data[i].offsetPostion != Vector2.zero ? data[i].position + data[i].offsetPostion : data[i].position);
                var propData = propDatabase.Get(message.Names[i]);

                if (propData?.propCollider is null || propData.Value.propCollider.Length <= 0)
                    continue;
                
                int positionHash = 0;

                for (int j = 0; j < propData.Value.propCollider.Length; j++)
                {
                    int hashCheck;
                    int newPositionHash = message.Positions[i];
                    int newScaleHash = message.Scales[i].GetHashCode();
                    int newRotationHash = message.Rotations[i].GetHashCode();
                    hashCheck = HashCode.Combine(newPositionHash, newRotationHash, newScaleHash);

                    if (hashDict.ContainsKey(hashCheck))
                    {
                        int hash;
                        positionHash = positionHash != 0 ? HashCode.Combine(positionHash, hashDict[hashCheck].transform.position.GetHashCode()) : hashDict[hashCheck].transform.position.GetHashCode();
                        int scaleHash = hashDict[hashCheck].transform.localScale.GetHashCode();
                        int rotationHash = propData.Value.propObject.transform.rotation.GetHashCode();
                        hash = HashCode.Combine(positionHash, rotationHash, scaleHash);

                        if (hash == hashCheck)
                            continue;

                        Object.Destroy(hashDict[hashCheck].gameObject);
                        hashDict.Remove(hashCheck);
                    }

                    var rotation = propData.Value.propCollider[j].transform.rotation;

                    if (data[i].flip)
                        rotation.z *= -1;

                    var tempColls = new List<Collider2D>();
                    var collider = colliderFactory.Create(propData.Value.propCollider[j]);
                    collider.transform.position = (Vector2)propData.Value.propCollider[j].transform.position * data[i].scale + data[i].position;
                    collider.transform.rotation = rotation;
                    collider.transform.localScale = data[i].scale;

                    hashDict.TryAdd(hashCheck, collider);

                    if (!colliders.ContainsKey(index))
                    {
                        tempColls.Add(collider);
                        colliders.Add(index, tempColls.ToArray());
                        continue;
                    }

                    tempColls = colliders[index].ToList();
                    tempColls.Add(collider);
                    colliders[index] = tempColls.ToArray();
                }
            }
        }
    }
}