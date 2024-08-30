using TMPro;
using Mirror;
using Zenject;
using UnityEngine;
using com.playbux.avatar;
using System.Collections;
using com.playbux.events;
using com.playbux.identity;
using com.playbux.settings;
using com.playbux.schedulessetting;

namespace com.playbux.networking.networkavatar
{
    public sealed class NetworkAvatarController : AvatarController<uint, NetworkIdentity>, IIdentityObserver
    {
        [SerializeField]
        private Camera textureCamera;

        [SerializeField]
        private Animator shadowAnimator;

        [SerializeField]
        private TextMeshPro[] nameText;

        [SerializeField]
        private GameObject playerMark;

        private int lastDirection;
        private uint clientNetworkId;
        private SignalBus signalBus;
        private NetworkAvatarBoard board;
        private IIdentitySystem identitySystem;
        private IAnimationInfo animationInfo;
        public int Direction { get => lastDirection; private set => lastDirection = value; }
        public IAnimationInfo AnimationInfo { get => animationInfo; private set => animationInfo = value; }



#if !SERVER
        [Inject]
        private void SetUP(NetworkAvatarBoard board, IIdentitySystem identitySystem, SignalBus signalBus)
        {
            this.board = board;
            this.identitySystem = identitySystem;

            if (AvatarId.isOwned)
            {
                clientNetworkId = 0;
            }
            this.signalBus = signalBus;
            this.signalBus.Subscribe<SettingDataSignal>(OnSettingDataSignalRecieve);
        }

        public override void OnAvatarChanged(NetworkIdentity playerId, IAvatarSet newAvatar)
        {
            base.OnAvatarChanged(playerId, newAvatar);
            if (clientNetworkId.Equals(GetId(playerId)))
            {
                board.OnUpdateEquiped?.Invoke();
            }
        }
        public override void OnAvatarChanged(uint playerId, IAvatarSet newAvatar)
        {
            base.OnAvatarChanged(playerId, newAvatar);

            if (clientNetworkId.Equals(playerId))
            {
                board.OnUpdateEquiped?.Invoke();
            }
        }

        public override void OnDirectionChanged(NetworkIdentity playerId, int newDirection)
        {
            
            base.OnDirectionChanged(playerId, newDirection);
            if (!clientNetworkId.Equals(playerId))
                return;
            lastDirection = newDirection;
        }
        public override void OnDirectionChanged(uint playerId, int newDirection)
        {
            
            base.OnDirectionChanged(playerId, newDirection);
            if (!clientNetworkId.Equals(playerId))
                return;
            lastDirection = newDirection;
        }

        public override void OnAnimationChanged(NetworkIdentity playerId, IAnimationInfo newAnimation)
        {
            base.OnAnimationChanged(playerId, newAnimation);
            if (!clientNetworkId.Equals(playerId))
                return;
            animationInfo =  newAnimation;
        }

        public override void OnAnimationChanged(uint playerId, IAnimationInfo newAnimation)
        {
            base.OnAnimationChanged(playerId, newAnimation);
            if (!clientNetworkId.Equals(playerId))
                return;
            animationInfo = newAnimation;
        }

        protected override void AdditionalAnimationChanged(IAnimationInfo newAnimation)
        {
            var clipName = newAnimation.GetAnimationName();
            string animationName = "Shadow_" + clipName + "_" + lastDirection;
            shadowAnimator.Play(animationName, 0);
        }

        public void Start()
        {
            if (AvatarId.isOwned)
            {
                clientNetworkId = 0;
                /*var rigid =  gameObject.AddComponent<Rigidbody2D>();
                rigid.gravityScale = 0;
                gameObject.AddComponent<CircleCollider2D>();*/


            }
            else
            {

            }

            StartCoroutine(EStart());
        }

        private IEnumerator EStart()
        {
            yield return new WaitUntil(() => AvatarId.netId != 0);

            identitySystem[AvatarId.netId].GameObject = this.gameObject;
            identitySystem[AvatarId.netId].Identity = AvatarId;
            identitySystem[AvatarId.netId].Observer = this;


            if (AvatarId.isOwned)
            {
                clientNetworkId = AvatarId.netId;

                if (textureCamera != null)
                {
                    textureCamera.enabled = true;
                }
                board.SetLocalAvatarLayer(AvatarId.netId, LayerMask.NameToLayer("Capture"));
                
            }
            else
            {
                if (textureCamera != null)
                {
                    Destroy(textureCamera.gameObject);
                }
                if (playerMark != null)
                {
                    Destroy(playerMark);
                }
            }

            board.GetAvatarSet(AvatarId);
            yield return new WaitForSeconds(0.5f);
            identitySystem.GetUserdata(AvatarId);

        }
#else

        protected override void AdditionalAnimationChanged(IAnimationInfo newAnimation)
        {

        }

#endif
        protected override uint GetId(NetworkIdentity reference)
        {
            return reference.netId;
        }

        public void OnUpdateProfile(IdentityDetail detail)
        {
            string name = detail.DisplayName;

            if (detail.ID == detail.DisplayName)
            {
                name = "NewBux" + detail.NetId;
            }

            //must order 0 1
            nameText[0].text = string.Format("<mark=#00000090><alpha=#00>1<space=0em>{0}<space=0em><alpha=#00>1</mark>", name);
            nameText[1].text = string.Format("{0}", name);

            if (AvatarId.isOwned)
            {
                //FIXME: try signal
                var myNameDisplaySetting = SettingDataStatic.settingDataBase.MyNameDisplaySetting;
                nameText[0].gameObject.SetActive(myNameDisplaySetting.IsShow);
                nameText[1].gameObject.SetActive(myNameDisplaySetting.IsShow);
                nameText[1].color = myNameDisplaySetting.Color;
            }
            else
            {
                var otherNameDisplaySetting = SettingDataStatic.settingDataBase.OtherNameDisplaySetting;
                nameText[0].gameObject.SetActive(otherNameDisplaySetting.IsShow);
                nameText[1].gameObject.SetActive(otherNameDisplaySetting.IsShow);
                nameText[1].color = otherNameDisplaySetting.Color;
            }
        }

        public void OnSettingDataSignalRecieve(SettingDataSignal signal)
        {
            var command = signal.Command;
            if (command == "MyNameDisplaySetting")
            {
                if (AvatarId.isOwned)
                {
                    DisplayNameSetting displayNameSetting = (DisplayNameSetting)signal.Data;
                    nameText[0].gameObject.SetActive(displayNameSetting.IsShow);
                    nameText[1].gameObject.SetActive(displayNameSetting.IsShow);
                    nameText[1].color = displayNameSetting.Color;
                    return;
                }

            }
            else if (command == "OtherNameDisplaySetting")
            {
                if (AvatarId.isOwned)
                {

                }
                else
                {
                    DisplayNameSetting displayNameSetting = (DisplayNameSetting)signal.Data;
                    nameText[0].gameObject.SetActive(displayNameSetting.IsShow);
                    nameText[1].gameObject.SetActive(displayNameSetting.IsShow);
                    nameText[1].color = displayNameSetting.Color;
                }
                return;
            }
        }
    }
}