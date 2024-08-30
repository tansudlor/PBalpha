using System;
using Mirror;
using UnityEngine;
using com.playbux.enemy;
using com.playbux.entities;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.core
{
    public class AdminEnemySpawnCommandWorker : BaseCommandWorker
    {
        private readonly EnemyAssetDatabase assetDatabase;
        private readonly ICredentialProvider credentialProvider;
        private readonly NetworkEnemyFactory networkEnemyFactory;

        public AdminEnemySpawnCommandWorker(
            CommandInstruction instruction,
            EnemyAssetDatabase assetDatabase,
            // ICredentialProvider credentialProvider,
            NetworkEnemyFactory networkEnemyFactory) : base(instruction)
        {
            // this.credentialProvider = credentialProvider;
            this.assetDatabase = assetDatabase;
            this.networkEnemyFactory = networkEnemyFactory;
        }

        internal override bool ValidateParameterAmount(string[] parameters)
        {
            return parameters.Length >= Instruction.Parameters.Length;
        }

        internal override bool ValidateParameterType(string[] parameters)
        {
            return parameters.Length >= 1 && IsNumber(parameters[0]);
        }

        public override void Perform(int connectionId, params string[] parameters)
        {
            long ticks = DateTime.Now.Ticks;
            var connection = NetworkServer.connections[connectionId];

            if (!ValidateParameterAmount(parameters) || !ValidateParameterType(parameters))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Invalid parameter input for {Instruction.Name} command.");
                connection.Send(errorMessage);

                return;
            }

            if (parameters.Length > 0 && string.IsNullOrEmpty(parameters[0]))
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Message cannot be empty for '{Instruction.Name}' command.");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: message cannot be empty.");
#endif
                return;
            }

            uint id = uint.Parse(parameters[0]);
            var asset = assetDatabase.Get(id);
            if (asset == null)
            {
                var errorMessage = new ChatBroadcastMessage(
                    ticks,
                    (ushort)ChatLevel.Warning,
                    "System",
                    $"Command '{Instruction.Name}' error.");
                connection.Send(errorMessage);

#if DEVELOPMENT
                Debug.Log($"{Instruction.Name} command: enemy {id} asset is null.");
#endif
                return;
            }

            var enemyEntity = networkEnemyFactory.Create(asset, connection.identity.transform.position);
            HandleNetworkIdentity(id, enemyEntity).Forget();
        }

        private async UniTaskVoid HandleNetworkIdentity(uint enemyId, IEntity<EnemyIdentity> enemy)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1 * 0.33f), ignoreTimeScale: false);
            await UniTask.WaitUntil(() => enemy.Identity.NetworkIdentity.netId != 0);

            // foreach (var pair in NetworkServer.connections)
            // {
            //     pair.Value.Send(new EnemySpawnMessage(enemy.Identity.NetworkIdentity.netId, enemyId));
            // }
            
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            enemy.Initialize();
        }
    }
}