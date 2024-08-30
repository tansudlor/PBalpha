using UnityEngine;
using Zenject;
using com.playbux.input;
using Zenject;

namespace com.playbux.ui.interactdialog
{
    public class InteractDialogInstaller : MonoInstaller<InteractDialogInstaller>
    {
        [SerializeField]
        private InteractDialog interactDialog;

        public override void InstallBindings()
        {
            //FIXME: doesn't have to couple with the quest system
            Container.Bind<InteractDialog>().FromInstance(interactDialog).AsSingle();

            //Container.Bind<IInputController>().To<InteractDialogInputController>().AsSingle();
        }
    }
}
