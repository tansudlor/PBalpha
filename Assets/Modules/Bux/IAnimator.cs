namespace com.playbux.bux
{
    public interface IAnimator
    {
        float Speed { set; }
        bool IsAnimationPlaying(string animationName);
        string GetCurrentAnimationName();
        void ClearCurrentAnimationName();
        void Play(string animationName, bool loop); // play with loop control by coding
        void Play(string animationName); // play without control animation use only animation attribute.
        void Stop(string animationName);
        void StopAll();
        void Dispose();
    }
}