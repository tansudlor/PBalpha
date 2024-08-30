using DG.Tweening;
using UnityEngine;

namespace com.playbux.networking.mirror.client.chat
{
    public class ChatUIFader
    {
        private readonly float duration = 0.4f;
        private readonly Ease ease = Ease.InOutCirc;
        private readonly CanvasGroup canvasGroup;

        private Tween currentTween;

        public ChatUIFader(CanvasGroup canvasGroup)
        {
            this.canvasGroup = canvasGroup;
        }

        public void FadeIn()
        {
            currentTween?.Kill();
            canvasGroup.interactable = true;
            currentTween = canvasGroup.DOFade(1, duration).SetEase(ease);
        }

        public void FadeOut()
        {
            currentTween?.Kill();
            canvasGroup.interactable = false;
            currentTween = canvasGroup.DOFade(0, duration).SetEase(ease);
        }

        private void OnComplete()
        {
            canvasGroup.interactable = true;
        }
    }

}