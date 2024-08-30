using Zenject;
using UnityEngine;

namespace com.playbux.input
{
    [CreateAssetMenu(menuName = "Playbux/Input/Create InputInstaller", fileName = "InputInstaller", order = 0)]
    public class InputInstaller : ScriptableObjectInstaller<InputInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<PlayerControls>().AsSingle();
        }
    }
}