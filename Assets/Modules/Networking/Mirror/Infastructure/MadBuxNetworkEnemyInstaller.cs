using Zenject;
using UnityEngine;
using Spine.Unity;
using com.playbux.bux;
using com.playbux.enemy;
using com.playbux.entities;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.entity;
using com.playbux.networking.client.ability;
using com.playbux.networking.mirror.client.enemy;
using com.playbux.networking.mirror.server.enemy;

namespace com.playbux.networking.mirror.infastructure
{
    public class MadBuxNetworkEnemyInstaller : MonoInstaller<MadBuxNetworkEnemyInstaller>
    {
        [SerializeField]
        private EnemyIdentity identity;

        [SerializeField]
        private EnemyAbilityData abilityData;

        [SerializeField]
        private AbilityAnimationName<string> abilityAnimationName;

        [SerializeField]
        private SkeletonAnimation skinPrefab;

        public override void InstallBindings()
        {
            Container.Bind<EnemyEnmityList>().AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.Bind<EnemyIdentity>().FromInstance(identity).AsSingle();
            Container.Bind<EnemyAbilityData>().FromInstance(abilityData).AsSingle();
            Container.Bind<IEntity<EnemyIdentity>>().To<TargetableNetworkEnemyEntity>().AsSingle();

#if SERVER
            BindServerSide();
#else
            BindClientSide();
#endif
        }

        private void BindClientSide()
        {
#if !SERVER
            Container.Bind<IAnimator>().To<SpineAnimator>().AsSingle();
            Container.Bind<AbilityAnimationName<string>>().FromInstance(abilityAnimationName).AsSingle();
            Container.Bind<SkeletonAnimation>().FromComponentInNewPrefab(skinPrefab).UnderTransform(transform).AsSingle();
            Container.BindFactory<EnemyIdentity, IEnemyClientBehaviour, IEnemyClientBehaviour.Factory>().FromFactory<MadBuxEnemyClientBehaviourFactory>();
#endif
        }

        private void BindServerSide()
        {
#if SERVER
            Container.Bind<IEnemyServerBehaviour>().To<MadBuxEnemyServerBehaviour>().AsSingle();
#endif
        }
    }
}