using System;
using DG.Tweening;
using UnityEngine;

namespace com.playbux.networking.client.ability
{
    public class CircleTelegraphAnimator : ITelegraphAnimator
    {
        private readonly SpriteRenderer telegraphRenderer;

        public CircleTelegraphAnimator(SpriteRenderer telegraphRenderer)
        {
            this.telegraphRenderer = telegraphRenderer;
        }

        public void Play(Action onComplete = null)
        {
            telegraphRenderer.DOFade(0.7f, 0.4f).OnComplete(() => onComplete?.Invoke());
        }

        public void Stop(Action onComplete = null)
        {
            telegraphRenderer.DOFade(0f, 0.4f).OnComplete(() => onComplete?.Invoke());
        }
    }
}