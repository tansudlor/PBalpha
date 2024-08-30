using Zenject;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using com.playbux.bux;
using com.playbux.avatar;
using com.playbux.events;
using com.playbux.entities;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Animator = UnityEngine.Animator;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.entity;
using com.playbux.networking.mirror.message;
using com.playbux.networking.mirror.client.fakeplayer;
using com.playbux.networking.mirror.server.fakeplayer;

namespace com.playbux.networking.mirror.infastructure
{
    public class NetworkFakePlayerInstaller : MonoInstaller<NetworkFakePlayerInstaller>
    {
        [SerializeField]
        private Transform bux4Direction;

        [SerializeField]
        private Animator shadowAnimator;

        [SerializeField]
        private SortingGroup sortingGroup;

        [SerializeField]
        private NavMeshAgent navMeshAgent;

        [SerializeField]
        private FakePlayerIdentity identity;

        [SerializeField]
        private GameObjectContext partSwapperContext;

        [SerializeField]
        private GameObjectContext changeNameContext;

        [SerializeField]
        private GameObjectContext moveStateContext;

        [SerializeField]
        private GameObjectContext changeEquipmentContext;

        [SerializeField]
        private Animator[] directionAnimators;

        public override void InstallBindings()
        {
            navMeshAgent.updateUpAxis = false;
            navMeshAgent.updateRotation = false;
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.BindInterfacesAndSelfTo<InternalUpdateWorker>().AsSingle();
            Container.Bind<FakePlayerIdentity>().FromInstance(identity).AsSingle();
            Container.Bind<NetworkFakePlayerFacade>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<NetworkIdentity>().FromInstance(identity.NetworkIdentity).AsSingle();
            Container.Bind<IEntity<FakePlayerIdentity>>().To<NetworkFakePlayerEntity>().AsSingle();
#if SERVER
            Container.Bind<NavMeshAgent>().FromInstance(navMeshAgent).AsSingle();
            Container.BindInterfacesAndSelfTo<SimpleFakePlayerServerBehaviour>().AsSingle();
            Container.Bind<IFakePlayerState>().FromSubContainerResolve().ByNewContextPrefab(moveStateContext).AsTransient();
            Container.Bind<IFakePlayerState>().FromSubContainerResolve().ByNewContextPrefab(changeEquipmentContext).AsTransient();
            Container.Bind<IServerMessageSender<FakePlayerNameChangeMessage>>().FromSubContainerResolve().ByNewContextPrefab(changeNameContext).AsSingle();
#else
            navMeshAgent.enabled = false;
            Container.Bind<PartDirectionWorker>().AsSingle();
            Container.Bind<IAnimator>().To<AvatarAnimator>().AsSingle();
            Container.Bind<Transform>().FromInstance(bux4Direction).AsSingle();
            Container.Bind<Animator>().FromInstance(shadowAnimator).AsSingle();
            Container.Bind<SortingGroup>().FromInstance(sortingGroup).AsSingle();
            Container.Bind<Animator[]>().FromInstance(directionAnimators).AsSingle();
            Container.BindInterfacesAndSelfTo<OtherFakePlayerClientBehaviour>().AsSingle();
            Container.Bind<PartSwapper>().FromSubContainerResolve().ByNewContextPrefab(partSwapperContext).AsSingle();
            Container.BindSignal<SettingDataSignal>().ToMethod<OtherFakePlayerClientBehaviour>(behaviour => behaviour.OnSettingDataSignalRecieve).FromResolveAll();
#endif
        }
    }
}