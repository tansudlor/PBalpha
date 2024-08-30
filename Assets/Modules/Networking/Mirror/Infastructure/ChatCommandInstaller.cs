using Zenject;
using UnityEngine;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.infastructure
{
    public class ChatCommandInstaller<T> : MonoInstaller<ChatCommandInstaller<T>> where T : ICommandWorker
    {
        [SerializeField]
        private CommandInstruction instruction;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<T>().AsSingle();
            Container.Bind<CommandInstruction>().FromInstance(instruction).AsSingle();
        }
    }
}