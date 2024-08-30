using Zenject;
using UnityEngine;
using com.playbux.action;

namespace com.playbux.networking.mirror.client.action
{
    public class ClientActionAreaInstaller : MonoInstaller<ClientActionAreaInstaller>
    {
        [SerializeField]
        private ClientActionDatabase database;

        public override void InstallBindings()
        {
#if !SERVER
            Container.Bind<ClientActionDatabase>().FromInstance(database).AsSingle();
            Container.BindInterfacesAndSelfTo<ClientActionAreaController>().AsSingle();
            Container.BindFactory<Vector2, Object, IActionArea, IActionArea.Factory>().FromFactory<ActionAreaFactory>();
#endif
        }
    }
}