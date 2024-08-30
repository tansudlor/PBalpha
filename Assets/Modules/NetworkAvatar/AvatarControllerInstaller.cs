using Zenject;
using UnityEngine;
using com.playbux.bux;
using com.playbux.avatar;
using Animator = UnityEngine.Animator;

namespace com.playbux.networking.networkavatar
{
    public class AvatarControllerInstaller : MonoInstaller<AvatarControllerInstaller>
    {
        [SerializeField]
        private Animator[] directions;

        [SerializeField]
        private AvatarPartsCollection[] resources;

        public override void InstallBindings()
        { 
            Container.Bind<IAnimator>().To<AvatarAnimator>().AsSingle();
            
#if !SERVER
            Container.Bind<Transform>().FromInstance(transform).AsSingle();
            Container.Bind<NetworkAvatarController>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.Bind<Animator[]>().FromInstance(directions).AsSingle();
            Container.Bind<AvatarPartsCollection[]>().FromInstance(resources).AsSingle();
#endif
        }
    }
}