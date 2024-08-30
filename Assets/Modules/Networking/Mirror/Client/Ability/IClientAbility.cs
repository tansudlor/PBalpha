using Zenject;
using UnityEngine;

namespace com.playbux.networking.client.ability
{
    public interface IClientAbility
    {
        bool IsLock { get; }
        bool IsCasting { get; }
        float CastTime { get; }
        void StartCast();
        void UpdateCast(float currentCastTime);
        void EndCast();
        void CancelCast();

        public class Factory : PlaceholderFactory<Object, Vector3, IClientAbility>
        {

        }
    }
}