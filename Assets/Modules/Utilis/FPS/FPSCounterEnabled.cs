using System;

namespace com.playbux.utilis.fps
{
    public struct FPSCounterEnabled : IEquatable<bool>, IEquatable<FPSCounterEnabled>
    {
        public readonly bool IsEnabled;
        public FPSCounterEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
        public bool Equals(bool other)
        {
            return IsEnabled == other;
        }
        public bool Equals(FPSCounterEnabled other)
        {
            return IsEnabled == other.IsEnabled;
        }
        public override bool Equals(object obj)
        {
            return obj is FPSCounterEnabled other && Equals(other);
        }
        public override int GetHashCode()
        {
            return IsEnabled.GetHashCode();
        }
    }
}