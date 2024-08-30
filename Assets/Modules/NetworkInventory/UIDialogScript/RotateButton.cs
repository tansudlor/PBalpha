using com.playbux.networking.networkavatar;
using Mirror;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.networkinventory
{
    public sealed class RotateButton : MonoBehaviour
    {
        private NetworkAvatarBoard board;
        private int direction;
        [Inject]
        public void Setup(NetworkAvatarBoard board)
        {
            this.board = board;
            direction = 1000000;
        }

        public void Rotate(int direction)
        {
            this.direction += direction;
            board.UpdateAvatarDirection(NetworkClient.localPlayer.netId, this.direction);
        }
    }
}