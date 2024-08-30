using System;
namespace com.playbux.networking.client.ability
{
    public interface ITelegraphAnimator
    {
        void Play(Action onComplete = null);
        void Stop(Action onComplete = null);
    }
}