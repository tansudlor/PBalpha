using Zenject;
using UnityEngine;
using System.Collections.Generic;

namespace com.playbux.avatar
{
    public class PartSwapperInstaller : MonoInstaller<PartSwapperInstaller>
    {
        [Inject]
        private Transform bux4Direction;
        
        [SerializeField]
        private AvatarPartsCollection[] partsCollections;

        public override void InstallBindings()
        {
            Container.Bind<PartSwapper>().AsSingle();
            Container.Bind<PartLayerWorker>().AsSingle();
            Container.Bind<AvatarPartsCollection[]>().FromInstance(partsCollections).AsSingle();
            Container.Bind<SpriteRenderer[]>().FromMethod(GetRenderers).AsSingle();
        }

        private SpriteRenderer[] GetRenderers()
        {
            return bux4Direction.GetComponentsInChildren<SpriteRenderer>(true);
        }
    }
}