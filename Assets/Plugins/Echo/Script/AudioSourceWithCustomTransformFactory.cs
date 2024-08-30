using UnityEngine;
using Zenject;

namespace SETHD.Echo
{
    public class AudioSourceWithCustomTransformFactory : IFactory<Transform, AudioSource>
    {
        private readonly DiContainer container;

        public AudioSourceWithCustomTransformFactory(DiContainer container)
        {
            this.container = container;
        }

        public AudioSource Create(Transform parent)
        {
            var instance = container.InstantiateComponentOnNewGameObject<AudioSource>(parent);
            return instance;
        }
    }
}