using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.world
{
    [CreateAssetMenu(menuName = "Playbux/UI/GameInWorldUIInstallerInstaller", fileName = "GameInWorldUIInstallerInstaller")]
    public class InWorldUIInstaller : ScriptableObjectInstaller<InWorldUIInstaller>
    {
        [SerializeField]
        private GameObject InteractBallonPrefab;
        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
#endif
        }

        private void BindClientSide()
        {
            Container.Bind<InteractBallon>().FromComponentInNewPrefab(InteractBallonPrefab).AsSingle();
        }
    }
}
