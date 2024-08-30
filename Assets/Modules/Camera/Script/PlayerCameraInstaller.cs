using Zenject;
using Cinemachine;
using UnityEngine;

namespace com.playbux.Camera
{
    public class PlayerCameraInstaller : MonoInstaller<PlayerCameraInstaller>
    {
        [SerializeField]
        private CinemachineVirtualCameraBase camera;
        
        public override void InstallBindings()
        {
            Container.Bind<ICamera>().To<PlayerCamera>().AsSingle();
            Container.Bind<Transform>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<ICinemachineCamera>().FromInstance(camera).AsSingle();
        }
    }
}