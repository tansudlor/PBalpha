using com.playbux.networking.networkavatar;
using Mirror;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.playbux.kicktowin
{
    public class KickToWinBallAnim : MonoBehaviour
    {
        [SerializeField]
        private SkeletonAnimation ballAnimation;

        public NetworkAvatarController Controller { get; set; }
        private string currentBallAnimation = "";
        private SortingGroup ballSorting;
        private SortingGroup playerSorting;

        private Vector3[] ballPositions = new Vector3[] { Vector2.right + Vector2.down * 0.5f, Vector2.left + Vector2.down * 0.5f, Vector2.left + Vector2.up * 0.5f, Vector2.right + Vector2.up * 0.5f };

        private void Start()
        {

        }
        private void Update()
        {
            

            if (Controller == null)
            {
                Debug.Log(Controller + " Controller");
                return;
            }

            
            if (Controller.AnimationInfo == null)
            {
                return;
            }

            ballSorting = ballAnimation.transform.parent.gameObject.GetComponent<SortingGroup>();
            playerSorting = NetworkClient.localPlayer.transform.Find("Bux4Direction Variant").GetComponent<SortingGroup>();
            ballAnimation.gameObject.transform.parent.position = NetworkClient.localPlayer.gameObject.transform.position + ballPositions[Controller.Direction] * 2;

            if (Controller.Direction > 1)
            {
                ballSorting.sortingOrder = playerSorting.sortingOrder - 1;
            }
            else
            {
                ballSorting.sortingOrder = playerSorting.sortingOrder + 1;
            }

            if (Controller.AnimationInfo.GetAnimationName() == avatar.ClipName.Idle)
            {

                if (currentBallAnimation != "Idle")
                {
                    ballAnimation.AnimationState.SetAnimation(0, "Idle", true);
                    currentBallAnimation = "Idle";

                }

            }
            else
            {
                if (currentBallAnimation != "Move_" + Controller.Direction)
                {
                    ballAnimation.AnimationState.SetAnimation(0, "Move_" + Controller.Direction, true);
                    currentBallAnimation = "Move_" + Controller.Direction;

                }

            }
        }

    }

}
