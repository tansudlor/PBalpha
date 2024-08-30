using System.Collections.Generic;
using Zenject;
using UnityEngine;

namespace com.playbux.networking.mirror.core
{
    [CreateAssetMenu(menuName = "Playbux/Command/Create CommandInstaller", fileName = "CommandInstaller", order = 0)]
    public class CommandInstaller : ScriptableObjectInstaller<CommandInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<ChatCommandProcessor>().AsSingle();
        }
    }

}