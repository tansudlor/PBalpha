using Zenject;
using UnityEngine;

namespace com.playbux.effects
{
    public class EffectInstaller : MonoInstaller<EffectInstaller>
    {
        [SerializeField]
        private EffectIconDatabase effectIconDatabase;

        [SerializeField]
        private TemporaryEffectDatabase tempEffectDatabase;

        [SerializeField]
        private PermanentEffectDatabase permanentEffectDatabase;

        public override void InstallBindings()
        {
            Container.Bind<EffectIconDatabase>().FromInstance(effectIconDatabase).AsSingle();
            Container.Bind<TemporaryEffectDatabase>().FromInstance(tempEffectDatabase).AsSingle();
            Container.Bind<PermanentEffectDatabase>().FromInstance(permanentEffectDatabase).AsSingle();
        }
    }
}