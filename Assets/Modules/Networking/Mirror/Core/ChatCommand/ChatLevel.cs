using System;

namespace com.playbux.networking.mirror.core
{
    [Serializable]
    public enum ChatLevel
    {
        Say = 0,
        Tell = 1,
        Shout = 2,
        Party = 3,
        Warning = 4,
        Announcement = 5,
    }
}