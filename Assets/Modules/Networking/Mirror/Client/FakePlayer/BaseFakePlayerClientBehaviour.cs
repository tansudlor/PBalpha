using Mirror;
using UnityEngine;
using com.playbux.avatar;
using com.playbux.bux;
using com.playbux.sorting;
using UnityEngine.Rendering;
using AnimationInfo = com.playbux.avatar.AnimationInfo;
using Animator = UnityEngine.Animator;

namespace com.playbux.networking.mirror.client.fakeplayer
{
    public abstract class BaseFakePlayerClientBehaviour : IFakePlayerClientBehaviour
    {
        private readonly IAnimator animator;
        private readonly Animator shadowAnimator;
        private readonly SortingGroup sortingGroup;
        private readonly NetworkIdentity networkIdentity;
        private readonly PartDirectionWorker partDirectionWorker;
        private readonly LayerSorterController layerSorterController;

        internal NetworkIdentity NetworkIdentity => networkIdentity;

        private bool isBack;
        private bool isLeft;
        private int lastIntDirection;
        private int currentIntDirection;
        private ClipName lastClipName;

        protected BaseFakePlayerClientBehaviour(
            IAnimator animator,
            Animator shadowAnimator,
            SortingGroup sortingGroup,
            NetworkIdentity networkIdentity,
            PartDirectionWorker partDirectionWorker,
            LayerSorterController layerSorterController)
        {
            this.animator = animator;
            this.sortingGroup = sortingGroup;
            this.shadowAnimator = shadowAnimator;
            this.networkIdentity = networkIdentity;
            this.partDirectionWorker = partDirectionWorker;
            this.layerSorterController = layerSorterController;
        }

        public abstract void Initialize();

        public abstract void Dispose();

        private int GetAnimationClip()
        {
            return isBack switch
            {
                false when isLeft => 0,
                false when !isLeft => 1,
                true when !isLeft => 2,
                true when isLeft => 3,
                _ => 0
            };
        }

        private void HandleWalkDirection(Vector2 currentDirection)
        {
            bool hasNoVerticalInput = currentDirection.y == 0;
            bool hasNoHorizontalInput = currentDirection.x == 0;
            isBack = hasNoVerticalInput ? isBack : currentDirection.y > 0;
            isLeft = hasNoHorizontalInput ? isLeft : currentDirection.x < 0;
        }

        internal void HandleSortingOrder()
        {
            if (networkIdentity == null)
                return;

            var sortables = layerSorterController.Get(networkIdentity.transform.position);

            if (sortables is null)
                return;

            if (sortables.Length <= 0)
                return;

            float distance = float.PositiveInfinity;
            int selectedIndex = -1;

            for (int i = 0; i < sortables.Length; i++)
            {
                var other = sortables[i].Distance(networkIdentity.transform.position);

                if (!other.HasValue)
                    continue;

                float delta = Vector2.Distance(networkIdentity.transform.position, other.Value);

                if (delta >= distance)
                    continue;

                distance = delta;
                selectedIndex = i;
            }

            if (selectedIndex < 0)
                return;

            int newSortingOrder = sortables[selectedIndex].GetSortOrder(networkIdentity.transform.position);
            sortingGroup.sortingOrder = newSortingOrder;
        }

        internal void HandleWalkAnimation(float acceleration, Vector2 currentDirection)
        {
            var speed = 1.5f + acceleration;
            HandleWalkDirection(currentDirection);

            bool isMovingHorizontally = !Mathf.Approximately(currentDirection.x, 0);
            bool isMovingVertically = !Mathf.Approximately(currentDirection.y, 0);

            var clipName = isMovingHorizontally || isMovingVertically ? ClipName.Walk : ClipName.Idle;

            if (clipName == ClipName.Idle)
                speed = 0.5f + acceleration;


            currentIntDirection = GetAnimationClip();

            if (lastIntDirection == currentIntDirection && lastClipName == clipName)
                return;

            partDirectionWorker.ChangeDirection(currentIntDirection);
            var animationInfo = new AnimationInfo(clipName, speed, PlayAction.Loop);
            HandleAnimation(animationInfo);
            lastClipName = clipName;
            lastIntDirection = currentIntDirection;
        }

        private void HandleAnimation(IAnimationInfo newAnimation)
        {
            animator.Speed = newAnimation.GetAnimationSpeed();
            PlayAction action = newAnimation.GetAnimationAction();
            string rawName = newAnimation.GetAnimationName().ToString();

            switch (action)
            {
                case PlayAction.Loop:
                    animator.Play(rawName, true);
                    break;
                case PlayAction.Play:
                    animator.Play(rawName, false);
                    break;
                case PlayAction.Pause:
                    animator.Stop(rawName);
                    break;
                case PlayAction.None:
                    animator.Play(rawName);
                    break;
            }

            var clipName = newAnimation.GetAnimationName();
            string animationName = "Shadow_" + clipName + "_" + lastIntDirection;
            shadowAnimator.Play(animationName, 0);
        }
    }
}