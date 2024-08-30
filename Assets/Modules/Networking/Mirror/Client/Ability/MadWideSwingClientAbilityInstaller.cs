using UnityEngine;

namespace com.playbux.networking.client.ability
{
    public class MadWideSwingClientAbilityInstaller : ClientAbilityInstallerBase<MadWideSwingClientAbility>
    {
        [SerializeField]
        private SpriteRenderer telegraphRenderer;

        protected override void AbilitySpecificBindings()
        {
            Container.Bind<ITelegraphAnimator>().To<CircleTelegraphAnimator>().AsSingle();
            Container.Bind<SpriteRenderer>().FromInstance(telegraphRenderer).AsSingle();
        }
    }
}