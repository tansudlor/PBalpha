using UnityEngine;
using Zenject;

namespace com.playbux.sfxwrapper
{
    [CreateAssetMenu(menuName = "Playbux/Sound/Create SFXWrapperInstaller", fileName = "SFXWrapperInstaller", order = 0)]
    public class SFXWrapperInstaller : ScriptableObjectInstaller<SFXWrapperInstaller>
    {
        // Start is called before the first frame update
        public override void InstallBindings()
        {
            Container.Bind<SFXWrapper>().AsSingle().NonLazy();
        }
    }
}
