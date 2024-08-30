using System;
using System.Collections.Generic;
using com.playbux.FATE;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server
{
    public class BuxPsychoFATEProcessor : BaseFATEProcessor
    {
        private HashSet<uint> netIds = new HashSet<uint>(20);

        public BuxPsychoFATEProcessor(uint id, IFATEScheduler scheduler, IServerMessageSender<FATEStartMessage> startSender) : base(id, scheduler, startSender)
        {

        }

        public override void Start(FATEData data)
        {
            throw new NotImplementedException();
        }

        public override void End()
        {
            throw new NotImplementedException();
        }
    }
}