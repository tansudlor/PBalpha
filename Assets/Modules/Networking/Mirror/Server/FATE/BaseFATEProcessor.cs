using com.playbux.FATE;
using com.playbux.networking.mirror.message;
using Zenject;

namespace com.playbux.networking.mirror.server
{
    public abstract class BaseFATEProcessor : IFATEProcessor, ILateDisposable
    {
        private readonly uint id;
        private readonly IFATEScheduler scheduler;
        private readonly IServerMessageSender<FATEStartMessage> startSender;
        
        public BaseFATEProcessor(uint id, IFATEScheduler scheduler, IServerMessageSender<FATEStartMessage> startSender)
        {
            this.id = id;
            this.scheduler = scheduler;
            this.startSender = startSender;
            this.scheduler.OnFATEStarted += Start;
        }
        
        public void LateDispose()
        {
            scheduler.OnFATEStarted -= Start;
        }

        public abstract void Start(FATEData data);

        public abstract void End();
    }
}