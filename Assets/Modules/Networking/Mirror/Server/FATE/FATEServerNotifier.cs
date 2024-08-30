using System;
using Zenject;
using com.playbux.FATE;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class FATEServerNotifier : ILateDisposable
    {
        private readonly IFATEScheduler scheduler;
        private readonly IServerMessageSender<FATENotificationMessage> notificationSender;
        
        public FATEServerNotifier(IFATEScheduler scheduler, IServerMessageSender<FATENotificationMessage> notificationSender)
        {
            this.scheduler = scheduler;
            this.notificationSender = notificationSender;
            this.scheduler.OnFATENotified += Notify;
        }
        
        public void LateDispose()
        {
            scheduler.OnFATENotified -= Notify;
        }

        private void Notify(DateTime time, FATEData data)
        {
            var dateTimeAsBytes = new DateTimeByte(Convert.ToByte(time.Hour), Convert.ToByte(time.Minute));
            string remainingTime = time.Hour <= 0 ? "" : time.Hour + (time.Hour < 2 ? " hour" : "hours");
            remainingTime += " " + time.Minute + (time.Minute < 2 ? " minute." : " minutes.");
            notificationSender.Send(new FATENotificationMessage(dateTimeAsBytes, data.id, $"{data.desc} expecting in {remainingTime}"));
        }
    }
}