using UnityEngine;
using Zenject;

namespace com.playbux.Camera
{
    [CreateAssetMenu(menuName = "Playbux/Camera/Create CameraInstaller", fileName = "CameraInstaller", order = 0)]
    public class CameraInstaller : ScriptableObjectInstaller<CameraInstaller>
    {
        [SerializeField]
        private GameObjectContext playerMainCameraContext;

        public override void InstallBindings()
        {
            Container.Bind<ICamera>().FromSubContainerResolve().ByNewContextPrefab(playerMainCameraContext).AsSingle();
        }
    }
}