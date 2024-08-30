using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.ability;

namespace com.playbux.networking.client.ability
{
    public class MadTightSwingClientAbility : MonoBehaviour, IClientAbility
    {
        public bool IsLock => isLock;
        public bool IsCasting => isCasting;
        public float CastTime => currentCastTime;

        private bool isLock;
        private bool isCasting;
        private float currentCastTime;
        private AbilityData abilityData;
        private ITelegraphAnimator animator;

        [Inject]
        private void Construct(ITelegraphAnimator animator, AbilityData abilityData)
        {
            this.animator = animator;
            this.abilityData = abilityData;
        }

        public void FixedUpdate()
        {
            if (!isCasting)
                return;

            currentCastTime += Time.deltaTime;
        }

        public void StartCast()
        {
            isCasting = isLock = true;
            TelegraphFadeIn();
        }

        public void UpdateCast(float currentCastTime)
        {
            float pong = (float)NetworkTime.rtt * 0.5f;
            float calculatedTime = currentCastTime + pong;

            if (calculatedTime >= abilityData.castTime)
            {
                TelegraphFadeOut();
                return;
            }

            if (calculatedTime >= currentCastTime)
            {
                this.currentCastTime = currentCastTime + pong;

                if (calculatedTime > abilityData.castTime - 0.5f)
                    return;

                TelegraphFadeIn();
            }
        }

        public void EndCast()
        {
            if (!isCasting)
                return;

            isCasting = isLock = false;
            TelegraphFadeOut();
            Destroy(gameObject);
        }

        public void CancelCast()
        {
            if (!isCasting)
                return;

            isCasting = false;
            animator.Stop(() => Destroy(gameObject));
        }

        private void TelegraphFadeIn()
        {
            animator.Play();
        }

        private void TelegraphFadeOut()
        {
            animator.Stop();
        }
    }
}