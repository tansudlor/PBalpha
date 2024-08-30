using System;
using Zenject;
using System.Linq;
using UnityEngine;
using com.playbux.events;
using com.playbux.entities;
using com.playbux.fakeplayer;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.utilis.network;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class FakePlayerServerSpawner : IInitializable, ILateDisposable, ILateTickable
    {
        private const float MIN_TIME_TO_SPAWN = 1f;
        private const float MAX_TIME_TO_SPAWN = 5f;
        private const string LIFE_TIME_KEY = "fake-player-life-time";
        private const string MAX_AMOUNT_KEY = "fake-player-max-amount";
        private const string MAX_TIME_SPAWN_KEY = "fake-player-max-time-to-spawn";
        private static readonly Vector3 spawnPosition = new Vector3(-18.5f, -45.5999985f, 0);

        private readonly SignalBus signalBus;
        private readonly FakePlayerAssetDatabase database;
        private readonly NetworkFakePlayerFactory factory;
        private readonly ServerNavMeshObject navMeshObject;

        private float spawnCounter = 0;
        private float despawnCounter = 0;
        private float configCounter = 0;
        private int maxAmount = 20;
        private int lifeTime = 30;
        private float timeToSpawnNext = -1;
        private float timeToDespawnNext = -1;
        private float maxTimeToSpawn = MAX_TIME_TO_SPAWN;
        private Dictionary<int, GameObject> activeAgents = new Dictionary<int, GameObject>();
        private Dictionary<int, GameObject> inactiveAgents = new Dictionary<int, GameObject>();
        private List<IEntity<FakePlayerIdentity>> despawnList = new List<IEntity<FakePlayerIdentity>>();
        private Dictionary<IEntity<FakePlayerIdentity>, float> runtimeActiveAgents = new Dictionary<IEntity<FakePlayerIdentity>, float>();

        public FakePlayerServerSpawner(SignalBus signalBus, FakePlayerAssetDatabase database, NetworkFakePlayerFactory factory, ServerNavMeshObject navMeshObject)
        {
            this.signalBus = signalBus;
            this.factory = factory;
            this.database = database;
            this.navMeshObject = navMeshObject;
        }

        public void Initialize()
        {
            for (int i = 0; i < database.Assets.Length; i++)
                inactiveAgents.Add(i, database.Assets[i]);

            GetNextSpawnTime();
            RequestConfigs();
        }

        public void LateDispose()
        {
            activeAgents.Clear();
            inactiveAgents.Clear();
        }

        public void LateTick()
        {
            SpawnTick();
            Despawn();
            LifeTimeTick();
            RequestConfigTick();
        }

        private void RequestConfigTick()
        {
            configCounter += Time.unscaledDeltaTime;

            if (configCounter < 30)
                return;

            RequestConfigs();
            configCounter = 0;
        }

        private void RequestConfigs()
        {
            signalBus.Fire(new RemoteConfigFetchRequestSignal(LIFE_TIME_KEY, (int)RemoteConfigType.Int));
            signalBus.Fire(new RemoteConfigFetchRequestSignal(MAX_AMOUNT_KEY, (int)RemoteConfigType.Int));
            signalBus.Fire(new RemoteConfigFetchRequestSignal(MAX_TIME_SPAWN_KEY, (int)RemoteConfigType.Float));
        }

        public void OnSpawnTimeConfigFetched(RemoteConfigResponseSignal<float> signal)
        {
            if (signal.key != MAX_TIME_SPAWN_KEY)
                return;

            maxTimeToSpawn = signal.value;
        }

        public void OnMaxAmountConfigFetched(RemoteConfigResponseSignal<int> signal)
        {
            if (signal.key != MAX_AMOUNT_KEY)
                return;

            maxAmount = signal.value;
        }

        public void OnLifeTimeConfigFetched(RemoteConfigResponseSignal<int> signal)
        {
            if (signal.key != LIFE_TIME_KEY)
                return;

            Debug.Log(lifeTime);
            lifeTime = signal.value;
        }

        private void SpawnTick()
        {
            if (runtimeActiveAgents.Count >= maxAmount)
                return;

            if (timeToSpawnNext < 0)
                return;

            spawnCounter += Time.unscaledDeltaTime;

            if (spawnCounter < timeToSpawnNext)
                return;

            Spawn();
        }

        private void Spawn()
        {
            var prefab = database.RandomAssetById.asset;

            if (inactiveAgents.Count > 0)
            {
                int randomIndex = Random.Range(0, database.Assets.Length);
                while (!inactiveAgents.ContainsKey(randomIndex))
                    randomIndex = Random.Range(0, database.Assets.Length);

                activeAgents.Add(randomIndex, database.Assets[randomIndex]);
                prefab = database.Assets[randomIndex];
                inactiveAgents.Remove(randomIndex);
            }

            var instance = factory.Create(prefab, spawnPosition);
            HandleSpawn(instance).Forget();
            runtimeActiveAgents.Add(instance, 0);
            GetNextSpawnTime();
        }

        private void Despawn()
        {
            foreach (var pair in runtimeActiveAgents)
            {
                if (pair.Value < lifeTime)
                    continue;

                despawnList.Add(pair.Key);
            }

            for (int i = 0; i < despawnList.Count; i++)
            {
                runtimeActiveAgents.Remove(despawnList[i]);
                despawnList[i].Dispose();
            }

            despawnList.Clear();
        }

        private void LifeTimeTick()
        {
            foreach (var pair in runtimeActiveAgents.Keys.ToArray())
            {
                runtimeActiveAgents[pair] += Time.deltaTime;
            }
        }

        private void GetNextSpawnTime()
        {
            if (timeToSpawnNext == -1)
            {
                timeToSpawnNext = 10;
                return;
            }

            spawnCounter = 0;
            timeToSpawnNext = Random.Range(MIN_TIME_TO_SPAWN, MAX_TIME_TO_SPAWN);
        }

        private async UniTaskVoid HandleSpawn(IEntity<FakePlayerIdentity> entity)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1 * 0.33f), ignoreTimeScale: false);
            await UniTask.WaitUntil(() => entity.Identity.NetworkIdentity.netId != 0);
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            entity.Initialize();
        }
    }
}