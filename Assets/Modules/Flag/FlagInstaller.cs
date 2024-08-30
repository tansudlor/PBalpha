using UnityEngine;
using Zenject;

namespace com.playbux.flag
{
    [CreateAssetMenu(menuName = "Playbux/Flag/FlagInstaller", fileName = "FlagInstaller")]
    public class FlagInstaller : ScriptableObjectInstaller<FlagInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IFlagCollection<string>>().To<FlagCollectionBase<string>>().AsSingle().NonLazy();

        }
    }
}
