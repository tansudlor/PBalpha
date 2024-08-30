using Mirror;
using UnityEngine;
using com.playbux.avatar;
using com.playbux.sorting;
using UnityEngine.Rendering;
using com.playbux.networking.networkavatar;
using AnimationInfo = com.playbux.avatar.AnimationInfo;

namespace com.playbux.networking.mirror.client
{
    public abstract class BasePlayerClientBehaviour : IClientBehaviour
    {
        private readonly NetworkAvatarBoard board;
        private readonly SortingGroup sortingGroup;
        private readonly NetworkIdentity networkIdentity;
        private readonly LayerSorterController layerSorterController;

        internal NetworkIdentity NetworkIdentity => networkIdentity;

        private bool isBack;
        private bool isLeft;

        protected BasePlayerClientBehaviour(
            NetworkAvatarBoard board,
            SortingGroup sortingGroup,
            NetworkIdentity networkIdentity,
            LayerSorterController layerSorterController)
        {
            this.board = board;
            this.sortingGroup = sortingGroup;
            this.networkIdentity = networkIdentity;
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

            board.UpdateAvatarDirection(networkIdentity.netId, GetAnimationClip());
            board.UpdateAvatarAnimation(networkIdentity.netId, new AnimationInfo(clipName, speed, PlayAction.Loop));
        }
    }
}