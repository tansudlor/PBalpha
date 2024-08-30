using System;
using System.Collections.Generic;

namespace com.playbux.FATE
{
    public interface IFATEScheduler
    {
        event Action<FATEData> OnFATEStarted;
        event Action<DateTime, FATEData> OnFATENotified;
        void Initialize();
        void Dispose();
        Dictionary<long, FATEScheduleData[]> Get();
        FATEScheduleData[] Get(long hourAndMinute);
        Dictionary<long, FATEScheduleData[]> Get(DayOfWeek day);
        FATEScheduleData[] Get(DayOfWeek day, long hourAndMinute);
    }
}