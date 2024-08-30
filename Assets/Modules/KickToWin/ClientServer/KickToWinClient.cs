#if !SERVER
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Zenject;
using com.playbux.events;
using System.ComponentModel;
using Cysharp.Threading.Tasks;
using com.playbux.networking.mirror.message;
using com.playbux.identity;
using System;
using com.playbux.networking.networkavatar;
using Spine;

namespace com.playbux.kicktowin
{
    public partial class KickToWin
    {
        private readonly INetworkMessageReceiver<KickToWinMessage> kickToWinMessageReceiver;

        private SignalBus signalBus;
        private DiContainer container;
        private CircleScript kickToWinScript;
        private IIdentitySystem identitySystem;
        private KickToWinTimmer kickToWinTimmerInPrefab;
        private KickToWinBallAnim kickToWinBallAnimInPrefab;
        private KickCoinController kickCoinController;
        private KickToWinUI kickToWinUI;

        private GameObject kickToWinPrefab;
        private GameObject kickToWinTimmerPrefab;
        private GameObject kickToWinBallPrefab;
        private GameObject kickCoinPrefab;

        private GameObject kickToWinInstance;

        private KickToWinTimmer kickToWinTimmerInstance;
        private KickToWinBallAnim kickToWinBallAnimInstance;
        private CircleScript circleScriptInstance;

       
        private bool youWin = false;



        public KickToWin(SignalBus signalBus, DiContainer diContainer, CircleScript kickToWinScript, INetworkMessageReceiver<KickToWinMessage> kickToWinMessageReceiver, IIdentitySystem identitySystem,
            KickToWinTimmer kickToWinTimmer, KickToWinBallAnim kickToWinBallAnim, KickToWinUI kickToWinUI, KickCoinController kickCoinController )
        {
            this.signalBus = signalBus;
            this.container = diContainer;
            this.kickToWinScript = kickToWinScript;

            this.kickToWinMessageReceiver = kickToWinMessageReceiver;
            kickToWinPrefab = kickToWinScript.gameObject;

            this.identitySystem = identitySystem;

            this.kickToWinTimmerInPrefab = kickToWinTimmer;
            kickToWinTimmerPrefab = kickToWinTimmer.gameObject;

            this.kickToWinBallAnimInPrefab = kickToWinBallAnim;
            kickToWinBallPrefab = kickToWinBallAnim.gameObject;

            this.kickToWinUI = kickToWinUI;

            this.kickCoinController = kickCoinController;
            kickCoinPrefab = kickCoinController.gameObject;

            this.kickToWinMessageReceiver.OnEventCalled += OnKickToWinMessageResponse;


            WaitForConnection().Forget();


        }

        async UniTask WaitForConnection()
        {
            await UniTask.WaitUntil(() => NetworkClient.active);
            //Debug.Log("OnStartClientSignal : Kick To Win");
            NetworkClient.RegisterSpawnHandler(kickToWinPrefab.GetComponent<NetworkIdentity>().assetId, SpawnKickPoint, DisposeKickPoint);
        }


        private GameObject SpawnKickPoint(SpawnMessage msg)
        {
            //Debug.Log("SpawnKickPoint : ");


            var circle = container.InstantiatePrefab(kickToWinPrefab);
            circleScriptInstance = circle.GetComponent<CircleScript>();
            /*var collider = circle.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;*/
            circleScriptInstance.BallSportlight.AnimationState.Complete += OnBallSportlightComplete;
            circleScriptInstance.SportLight.AnimationState.Complete += OnSportlightComplete;
            circleScriptInstance.BallSportlight.AnimationState.SetAnimation(0, "Spawn", false);
            circleScriptInstance.SportLight.AnimationState.SetAnimation(0, "Spawn", false);
            kickToWinInstance = circle;

            if (youWin)
            {
                circleScriptInstance.Appear = false;
            }

            return circle;
        }

        public void OnBallSportlightComplete(TrackEntry trackEntry)
        {
            try
            {
                //Debug.Log("trackEntry");
                if (trackEntry.Animation.Name == "Spawn")
                {

                    circleScriptInstance.BallSportlight.AnimationState.SetAnimation(0, "Idle", true);
                    circleScriptInstance.BallSportlight.AnimationState.Complete -= OnSportlightComplete;
                }
            }
            catch
            {
            }
        }

        public void OnSportlightComplete(TrackEntry trackEntry)
        {
            try
            {
                //Debug.Log("trackEntry");
                if (trackEntry.Animation.Name == "Spawn")
                {
                    circleScriptInstance.SportLight.AnimationState.SetAnimation(0, "Idle", true);
                    circleScriptInstance.SportLight.AnimationState.Complete -= OnSportlightComplete;
                }
            }
            catch
            {
            }
        }

        private void DisposeKickPoint(GameObject spawned)
        {
            //Debug.Log("DisposeKickPoint " + spawned.name);
            CircleScript cs = spawned.GetComponent<CircleScript>();

            cs.BallSportlight.AnimationState.SetAnimation(0, "End", false);
            cs.SportLight.AnimationState.SetAnimation(0, "End", false);

            cs.BallSportlight.AnimationState.Complete += (trackEntry) =>
            {
                GameObject.Destroy(spawned);
            };

        }

        private void DisposeKickPointWin(GameObject target)
        {
            //Debug.Log("DisposeKickPoint " + spawned.name);
            CircleScript cs = target.GetComponent<CircleScript>();

            cs.BallSportlight.AnimationState.SetAnimation(0, "End", false);
            cs.SportLight.AnimationState.SetAnimation(0, "End", false);

            cs.BallSportlight.AnimationState.Complete += (trackEntry) =>
            {
                target.transform.localScale = Vector3.zero;

            };

        }

        private void OnKickToWinMessageResponse(KickToWinMessage message)
        {
            var command = message.Message;
            Debug.Log(command);

            if (command == "closeball")
            {

                if (kickToWinBallAnimInstance != null)
                {
                    GameObject.Destroy(kickToWinBallAnimInstance.gameObject);
                    kickToWinBallAnimInstance = null;
                }
                youWin = false;
                return;
            }

            if (command == "clearuiall")
            {
                ClearUI();
                youWin = false;
                return;
            }


            if (NetworkClient.localPlayer.netId != message.NetId)
            {
                return;
            }

            if (command == "playerenter")
            {
                long enterTicks = message.enterTicks;
                long serverTicks = message.serverTicks;
                long stayTicks = serverTicks - enterTicks;

                kickToWinUI.On();
                kickToWinTimmerInstance = container
                    .InstantiatePrefab(kickToWinTimmerPrefab, identitySystem[message.NetId].Identity.gameObject.transform)
                    .GetComponent<KickToWinTimmer>();
                kickToWinTimmerInstance.StayTicks = stayTicks;
                kickToWinTimmerInstance.ArrivalTicks = DateTime.UtcNow.Ticks;

                kickToWinBallAnimInstance = container
                    .InstantiatePrefab(kickToWinBallPrefab)
                    .GetComponent<KickToWinBallAnim>();
                kickToWinBallAnimInstance.Controller = identitySystem[message.NetId].GameObject.GetComponent<NetworkAvatarController>();

                return;
            }

            if (command == "clearui")
            {
                ClearUI();
                return;
            }

            if (command == "youwin")
            {
                kickToWinUI.On();
                youWin = true;
                if (kickToWinTimmerInstance != null)
                {
                    kickToWinTimmerInstance.StartCoroutine(kickToWinTimmerInstance.DestoryTimmer());
                    kickToWinTimmerInstance = null;
                }

                //kickToWinUI.ScroeUP();
                DisposeKickPointWin(kickToWinInstance);
                return;
            }

            if (command.Contains("getscore"))
            {
                Debug.Log("getscore" + message.value);
                int score = message.value;

                kickToWinUI.On();
                CoinRush(score).Forget();
                //kickToWinUI.ScroeUP(score, true);

                return;
            }

            if (command.Contains("apierror"))
            {

                Debug.Log(command.Split(",")[1]);

                return;
            }

            if (command.Contains("getpebble"))
            {

                Debug.Log("getpebble" + message.value);
                identitySystem[NetworkClient.localPlayer.netId].BalancePebble += message.value;
                return;
            }

        }

        async UniTask CoinRush(int score)
        {
            for (int i = 0; i < score; i++)
            {

                GameObject kickCoinInstante = container.InstantiatePrefab(kickCoinPrefab, kickToWinUI.transform.parent);
                //Debug.Log("kickToWinUI.transform.localPosition" + kickToWinUI.transform.localPosition);
                kickCoinInstante.transform.localPosition = Vector3.zero;
                var kickCoinController = kickCoinInstante.GetComponent<KickCoinController>();
                kickCoinController.StartPosition = kickCoinInstante.transform.localPosition;
                kickCoinController.TargerPosition = kickToWinUI.transform.localPosition;
                kickCoinController.Order = i;
                kickCoinController.OnFinishMove = kickToWinUI.ScoreUp;
                kickCoinController.OnFinishMove += kickToWinUI.RoundScoreUp;
                await UniTask.Delay(100);
            }

            await UniTask.WaitForSeconds(3);
            kickToWinUI.Off();

        }


        void ClearUI()
        {
            if (kickToWinTimmerInstance != null)
            {
                kickToWinTimmerInstance.StartCoroutine(kickToWinTimmerInstance.DestoryTimmerNormal());
                kickToWinTimmerInstance = null;
            }


            if (kickToWinBallAnimInstance != null)
            {
                if (youWin == true)
                {

                }
                else
                {
                    GameObject.Destroy(kickToWinBallAnimInstance.gameObject);
                    kickToWinBallAnimInstance = null;
                }

            }

            kickToWinUI.Off();
        }
    }
}
#endif