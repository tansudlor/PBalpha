using System;
using Mirror;
namespace com.playbux.networking.mirror.message
{
    public struct DateTimeByte : IEquatable<DateTimeByte>, NetworkMessage
    {
        public readonly byte hour;
        public readonly byte minute;

        public DateTimeByte(byte hour, byte minute)
        {
            this.hour = hour;
            this.minute = minute;
        }

        public DateTimeByte(DateTime dateTime)
        {
            hour = Convert.ToByte(dateTime.Hour);
            minute = Convert.ToByte(dateTime.Minute);
        }

        public bool Equals(DateTimeByte other)
        {
            return hour == other.hour && minute == other.minute;
        }
        public override bool Equals(object obj)
        {
            return obj is DateTimeByte other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(hour, minute);
        }
    }
}
