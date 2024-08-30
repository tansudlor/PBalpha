using com.playbux.ability;
using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.client.ability
{
    public class MadWideSwingClientAbility : MonoBehaviour, IClientAbility
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

            //TODO: need to play animation here
        }

        public void CancelCast()
        {
            if (!isCasting)
                return;

            isCasting = false;
            animator.Stop();
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