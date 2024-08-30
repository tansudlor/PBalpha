using System.Linq;
using Spine.Unity;
using System.Collections.Generic;

namespace com.playbux.bux
{
    public class SpineAnimator : IAnimator
    {
        public float Speed
        {
            set => skin.timeScale = value;
        }

        private string lastAnimation;
        private SkeletonAnimation skin;
        private Dictionary<string, int> trackNumbers = new();

        public SpineAnimator(SkeletonAnimation skin)
        {
            this.skin = skin;
        }

        public bool IsAnimationPlaying(string animationName)
        {
            return trackNumbers.ContainsKey(animationName);
        }

        public void Play(string animationName, bool loop)
        {
            bool hasKey = trackNumbers.ContainsKey(animationName);
            int trackIndex = hasKey ? trackNumbers[animationName] : trackNumbers.Count <= 0 ? 0 : trackNumbers.Values.Max();

            if (hasKey)
                return;

            trackNumbers.Add(animationName, trackIndex);
            skin.state.AddAnimation(trackIndex, animationName, loop, 0);
            lastAnimation = animationName;
        }

        public void Stop(string animationName)
        {
            bool hasKey = trackNumbers.ContainsKey(animationName);

            if (!hasKey)
                return;

            skin.state.SetEmptyAnimation(trackNumbers[animationName], 0);
            trackNumbers.Remove(animationName);
        }

        public void StopAll()
        {
            foreach (var pair in trackNumbers)
            {
                skin.state.SetEmptyAnimation(pair.Value, 0);
            }
        }

        public void Dispose()
        {
            foreach (var pair in trackNumbers)
            {
                skin.state.SetEmptyAnimation(pair.Value, 0);
            }
        }

        public string GetCurrentAnimationName()
        {
            return lastAnimation;
        }

        public void ClearCurrentAnimationName()
        {

        }

        public void Play(string animationName)
        {
            Play(animationName, false);
        }
    }
}