using UnityEngine;
using com.playbux.bux;
using System.Collections.Generic;
using Animator = UnityEngine.Animator;

namespace com.playbux.avatar
{
    public class AvatarAnimator : IAnimator
    {
        public float Speed { set => speed = value; }

        private float speed;
        private string currentAnimation;

        private HashSet<string> animationSet;
        private Animator[] animators;

        public AvatarAnimator(Animator[] directions)
        {
            animators = directions;
            animationSet = new HashSet<string>();
        }

        public void Dispose()
        {
            Debug.LogWarning(new System.NotImplementedException().ToString());
        }

        public bool IsAnimationPlaying(string animationName)
        {
            return animationName == currentAnimation;
        }

        public void Play(string animationName)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (!CheckIfAnimationExists(animators[i], animationName))
                    continue;

                if (currentAnimation == animationName)
                    continue;

                if (!animators[i].gameObject.activeInHierarchy)
                    continue;
                animators[i].speed = speed;
                animators[i].enabled = true;
                animators[i].Play(animationName);
                currentAnimation = animationName;
            }
        }


        public void Play(string animationName, bool loop = true)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (!CheckIfAnimationExists(animators[i], animationName))
                    continue;

                if (currentAnimation == animationName)
                    continue;

                if (!animators[i].gameObject.activeInHierarchy)
                    continue;

                animators[i].speed = speed;
                animators[i].enabled = true;
                animators[i].SetBool("loop", loop);
                animators[i].Play(animationName);
                currentAnimation = animationName;
            }
        }

        public void Stop(string animationName)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                if (animators[i].GetCurrentAnimatorClipInfo(0)[0].clip.name != animationName)
                    continue;

                animators[i].enabled = false;
            }
        }

        public void StopAll()
        {
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].enabled = false;
            }
        }

        private bool CheckIfAnimationExists(Animator animator, string animationName)
        {
            if (animationSet.Count == 0)
            {
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                foreach (AnimationClip clip in clips)
                {
                    animationSet.Add(clip.name);
                }
            }
            return animationSet.Contains(animationName);
        }

        public string GetCurrentAnimationName()
        {
            return currentAnimation;
        }

        public void ClearCurrentAnimationName()
        {
            currentAnimation = "";
        }
    }
}