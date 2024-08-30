using System;
namespace com.playbux.networking.mirror.core
{
    public readonly struct CommandNameWrapper : IEquatable<CommandNameWrapper>
    {
        public readonly string Name;
        public readonly string AltName;

        public CommandNameWrapper(string name, string altName)
        {
            Name = name;
            AltName = altName;
        }
        public bool Equals(CommandNameWrapper other)
        {
            return Name == other.Name || AltName == other.AltName;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandNameWrapper other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AltName);
        }
    }
}