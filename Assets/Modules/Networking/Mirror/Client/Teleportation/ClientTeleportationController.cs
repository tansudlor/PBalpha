using System;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.ui;
using com.playbux.events;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using com.playbux.networking.mirror.core;

namespace com.playbux.network.mirror.client.telelportation
{
    public class ClientTeleportationController : IInitializable, ILateDisposable
    {
        private readonly TeleportPointDatabase database;
        private readonly TeleportableArea.Factory factory;

        private Dictionary<Vector2, (TeleportPositionData, TeleportableArea)> areaInstances;
        
        public ClientTeleportationController(
            TeleportPointDatabase database, 
            TeleportableArea.Factory factory)
        {
            this.factory = factory;
            this.database = database;
        }

        public void Initialize()
        {
            areaInstances = new Dictionary<Vector2, (TeleportPositionData, TeleportableArea)>();

            for (int i = 0; i < database.Data.Length; i++)
            {
                areaInstances.Add(database.Data[i].areaPosition,
                    new ValueTuple<TeleportPositionData, TeleportableArea>(database.Data[i], factory.Create(database.Data[i].areaPosition, database.Data[i].areaPrefab)));
            }
        }

        public void LateDispose()
        {
            // foreach (var pair in areaInstances)
            // {
            //     Object.Destroy(pair.Value.Item2.gameObject);
            // }
            //
            areaInstances.Clear();
        }
    }
}
