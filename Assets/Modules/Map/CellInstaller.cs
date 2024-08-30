using UnityEngine;
using Zenject;

namespace com.playbux.map
{
    public class CellInstaller : MonoInstaller<CellInstaller>
    {
        [SerializeField]
        private SpriteRenderer renderer;

        public override void InstallBindings()
        {
            Container.Bind<RuntimeCell>().AsSingle();
            Container.Bind<Transform>().FromInstance(transform).AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.Bind<SpriteRenderer>().FromInstance(renderer).AsSingle();
        }
    }
}