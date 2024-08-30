using System;

namespace com.playbux.bux
{
    public class Animator : IAnimator
    {
        float IAnimator.Speed { set => throw new NotImplementedException(); }

        public void ClearCurrentAnimationName()
        {
            throw new NotImplementedException();
        }

        public string GetCurrentAnimationName()
        {
            throw new NotImplementedException();
        }

        public void Play(string animationName)
        {
            throw new NotImplementedException();
        }

        void IAnimator.Dispose()
        {
            throw new NotImplementedException();
        }

        bool IAnimator.IsAnimationPlaying(string animationName)
        {
            throw new NotImplementedException();
        }

        void IAnimator.Play(string animationName, bool loop)
        {
            throw new NotImplementedException();
        }

        void IAnimator.Stop(string animationName)
        {
            throw new NotImplementedException();
        }

        void IAnimator.StopAll()
        {
            throw new NotImplementedException();
        }
    }
}