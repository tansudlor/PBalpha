namespace com.playbux.schedules
{



    public class TickData<TValue> : ITickData , IValueReader<TValue>
    {
        private long tick = 0;
        public TValue EventData;

        public long Tick { get => tick; set => tick = value; }

        public TickData(long tick, TValue eventData)
        {
            this.tick = tick;
            EventData = eventData;
        }

        public TValue Read()
        {
            return EventData;
        }
    }

}
