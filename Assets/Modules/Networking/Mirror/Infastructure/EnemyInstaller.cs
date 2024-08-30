using com.playbux.enemy;
using com.playbux.entities;
using com.playbux.networking.mirror.core;
using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.infastructure
{
    public class EnemyInstaller : MonoInstaller<EnemyInstaller>
    {
        [SerializeField]
        private EnemyAssetDatabase enemyAssetDatabase;

        public override void InstallBindings()
        {
            Container.Bind<EnemyAssetDatabase>().FromInstance(enemyAssetDatabase).AsSingle();
            Container.BindFactory<GameObject, Vector3, IEntity<EnemyIdentity>, NetworkEnemyFactory>().FromFactory<EnemyEntityFactory>();
        }
    }
}