using Zenject;
using UnityEngine;
using com.playbux.io;

namespace com.playbux.FATE
{
    public class FATEInstaller : MonoInstaller<FATEInstaller>
    {
        [SerializeField]
        private ScheduledFATEDatabase database;

        [SerializeField]
        private LocalScheduleJSONInstaller jsonInstaller;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LocalFileFATEScheduler>().AsSingle();
            Container.Bind<FileInfo>().FromInstance(database.FileInfo).AsSingle();
            Container.Bind<IAsyncFileReader<ScheduledFATEData>>()
                .FromSubContainerResolve()
                .ByNewContextPrefab(jsonInstaller)
                .AsSingle();
        }
    }

}