using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.server.fakeplayer
{
    public abstract class FakePlayerStateInstaller<T> : MonoInstaller<FakePlayerStateInstaller<T>>
    {
        [SerializeField]
        private FakePlayerStateEnum stateEnum;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle();
            Container.Bind<FakePlayerStateEnum>().FromInstance(stateEnum).AsSingle();
            AdditionalBindings();
        }

        public abstract void AdditionalBindings();
    }
}