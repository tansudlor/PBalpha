using System;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.events;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.teleportation
{
    public class ServerTeleportationController : IInitializable, ILateDisposable
    {
        private readonly SignalBus signalBus;
        private readonly TeleportPointDatabase database;
        private readonly TeleportableArea.Factory factory;

        private Dictionary<Vector2, (TeleportPositionData, TeleportableArea)> areaInstances;
        public ServerTeleportationController(SignalBus signalBus, TeleportPointDatabase database, TeleportableArea.Factory factory)
        {
            this.factory = factory;
            this.database = database;
            this.signalBus = signalBus;
        }

        public void Initialize()
        {
            areaInstances = new Dictionary<Vector2, (TeleportPositionData, TeleportableArea)>();

            for (int i = 0; i < database.Data.Length; i++)
            {
                areaInstances.Add(database.Data[i].areaPosition,
                    new ValueTuple<TeleportPositionData, TeleportableArea>(database.Data[i], factory.Create(database.Data[i].areaPosition, database.Data[i].areaPrefab)));
            }

            signalBus.Subscribe<TeleportationInitiateSignal>(OnSignalReceived);
        }

        public void LateDispose()
        {
            areaInstances.Clear();
            signalBus.Unsubscribe<TeleportationInitiateSignal>(OnSignalReceived);
        }

        private void OnSignalReceived(TeleportationInitiateSignal signal)
        {
            bool isValid = NetworkServer.spawned.ContainsKey(signal.netId) && areaInstances.ContainsKey(signal.keyPosition) && areaInstances[signal.keyPosition].Item2.Validate(signal.projectedPosition);

            if (!isValid)
            {
                NetworkServer.spawned[signal.netId].connectionToClient.Send(new TeleportationInvalidMessage(signal.netId));
                return;
            }

            foreach (var pair in NetworkServer.spawned[signal.netId].observers)
            {
                if (pair.Key == signal.netId)
                    continue;

                pair.Value.Send(new TeleportationValidMessage(signal.netId, areaInstances[signal.keyPosition].Item2.IsInside, areaInstances[signal.keyPosition].Item1.targetPosition));
            }

            signalBus.Fire(new TeleportationCompleteSignal(signal.netId, areaInstances[signal.keyPosition].Item1.targetPosition));
            NetworkServer.spawned[signal.netId].connectionToClient.Send(new TeleportationValidMessage(signal.netId, areaInstances[signal.keyPosition].Item2.IsInside, areaInstances[signal.keyPosition].Item1.targetPosition));
        }
    }
}