using UnityEngine;
using Zenject;
using com.playbux.input;

namespace com.playbux.networkquest
{
    public class QuestUIInstaller : MonoInstaller<QuestUIInstaller>
    {
        [SerializeField]
        private NPCDialogController uiController;

        public override void InstallBindings()
        {
            Container.Bind<NPCDialogController>().FromInstance(uiController).AsSingle();
            Container.Bind<IInputController>().To<QuestInputController>().AsSingle();
        }
    }
}