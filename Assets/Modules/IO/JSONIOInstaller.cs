using Zenject;

namespace com.playbux.io
{
    public abstract class JSONIOInstaller<T> : MonoInstaller<JSONIOInstaller<T>>
    {
        [Inject]
        private readonly FileInfo fileInfo;

        public override void InstallBindings()
        {
            Container.Bind<IOFacade<T>>().AsSingle();
            Container.Bind<FileInfo>().FromInstance(fileInfo).AsSingle();
            Container.Bind<IAsyncFileReader<T>>().To<JSONFileReader<T>>().AsSingle();
            Container.Bind<IAsyncFileWriter<T>>().To<JSONFileWriter<T>>().AsSingle();
        }
    }
}