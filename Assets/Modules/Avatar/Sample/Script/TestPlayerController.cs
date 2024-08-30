using com.playbux.avatar;
using com.playbux.input;
using com.playbux.utilis;
using com.playbux.utilis.math;
using Mirror;
using UnityEngine;
using Zenject;
using AnimationInfo = com.playbux.avatar.AnimationInfo;

namespace com.playbux.avatar.sample
{
    public class TestPlayerController : MonoBehaviour
    {
        private bool isBack;
        private bool isLeft;

        private float speed = 15f;
        private float diagonalSpeedMultiplier = 1.66f;

        private IAvatarBoard<uint, NetworkIdentity> board;
        private IInputController<Vector2> inputController;
        private NetworkIdentity id;

        [Inject]
        public void Setup(NetworkIdentity id, IAvatarBoard<uint, NetworkIdentity> board, IInputController<Vector2> inputController)
        {
            this.board = board;
            this.inputController = inputController;
            this.id = id;

            inputController.Enable();
        }

        void Update()
        {
            var input = inputController.Value;
            UpdatePosition(input);
            UpdateAnimation(input);
        }

        private void UpdatePosition(Vector2 direction)
        {
            float currentSpeed = speed;
            if (direction.x == 0 && direction.y != 0)
            {
                currentSpeed *= diagonalSpeedMultiplier;
            }

            Vector3 movement = direction.ToDirection() * currentSpeed * Time.deltaTime;
            transform.position += movement;
        }

        private void UpdateAnimation(Vector2 direction)
        {
            isBack = direction.y == 0 ? isBack : direction.y > 0;
            isLeft = direction.x == 0 ? isLeft : direction.x < 0;

            int dir = DetermineDirection();

            if (direction.magnitude > 0)
            {
                board.UpdateAvatarDirection(id, dir);
                board.UpdateAvatarAnimation(id, new AnimationInfo(ClipName.Walk, 3f, PlayAction.Loop));
            }
            else
            {
                board.UpdateAvatarAnimation(id, new AnimationInfo(ClipName.Idle, 1f, PlayAction.Loop));
            }

            if (Input.GetKey(KeyCode.Space))
            {
                board.UpdateAvatarAnimation(id, new AnimationInfo(ClipName.Walk, 3f, PlayAction.Pause));
            }
        }



        private int DetermineDirection()
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
    }
}
