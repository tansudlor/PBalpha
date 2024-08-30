using UnityEngine;
namespace com.playbux.networking.mirror.core
{
    public interface IPlayerCommand<T>
    {
        bool IsPreventOther { get; }
        ushort Id { get;}
        ushort TickCount { get; }
        int DataSize { get; }
        Vector2 FinalPosition { get; }
        byte[] Data { get; }
        T Perform();
        T Undo();
        IPlayerCommand<T> Clone();
    }
}