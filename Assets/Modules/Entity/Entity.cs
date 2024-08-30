using UnityEngine;
using Zenject;

namespace com.playbux.entities
{
    public interface IEntity<T>
    {
        uint TargetId { get; }
        T Identity { get; }
        GameObject GameObject { get; }
        void Initialize();
        void Dispose();
    }

    public class EntityFactory<T> : PlaceholderFactory<IEntity<T>>
    {

    }
}