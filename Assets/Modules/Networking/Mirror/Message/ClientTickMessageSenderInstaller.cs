using Mirror;
using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.message
{
    public abstract class ClientTickMessageSenderInstaller<T, T1> : MonoInstaller<ClientTickMessageSenderInstaller<T, T1>> where T : IClientMessageSender<T1> where T1 : struct, NetworkMessage
    {
        [Range(1, 120)]
        [SerializeField]
        private uint sendIntervalMultiplier = 1;
        
        public override void InstallBindings()
        {
            Container.Bind<uint>().FromInstance(sendIntervalMultiplier).AsSingle();
            Container.Bind(typeof(ILateTickable), typeof(IClientMessageSender<T1>)).To<T>().AsSingle();
        }

        public abstract void AdditionalBindings();
    }
}