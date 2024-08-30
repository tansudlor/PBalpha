using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.message
{
    public abstract class ServerTickMessageSenderInstaller<T, T1> : MonoInstaller<ServerTickMessageSenderInstaller<T, T1>> where T : IServerMessageSender<T1> where T1 : struct, NetworkMessage
    {
        [Range(1, 120)]
        [SerializeField]
        private uint sendIntervalMultiplier = 1;
        
        public override void InstallBindings()
        {
            Container.Bind<uint>().FromInstance(sendIntervalMultiplier).AsSingle();
            Container.Bind(typeof(ILateTickable), typeof(IServerMessageSender<T1>)).To<T>().AsSingle();
        }

        public abstract void AdditionalBindings();
    }
}