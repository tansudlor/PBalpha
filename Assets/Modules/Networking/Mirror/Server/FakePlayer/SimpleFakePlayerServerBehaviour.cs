using Faker;
using Mirror;
using Zenject;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class SimpleFakePlayerServerBehaviour : IFakePlayerServerBehaviour, ILateTickable
    {
        private readonly FakePlayerIdentity identity;
        private readonly NetworkIdentity networkIdentity;
        private readonly IServerMessageSender<FakePlayerNameChangeMessage> nameChangeMessageSender;
        private readonly Dictionary<FakePlayerStateEnum, IFakePlayerState> states;

        private float changeEquipmentTimer;
        public SimpleFakePlayerServerBehaviour(
            FakePlayerIdentity identity,
            NetworkIdentity networkIdentity,
            IServerMessageSender<FakePlayerNameChangeMessage> nameChangeMessageSender,
            List<IFakePlayerState> states)
        {
            this.identity = identity;
            this.networkIdentity = networkIdentity;
            this.nameChangeMessageSender = nameChangeMessageSender;
            this.states = new Dictionary<FakePlayerStateEnum, IFakePlayerState>();

            for (int i = 0; i < states.Count; i++)
                this.states.Add(states[i].StateEnum, states[i]);
        }

        public void Initialize()
        {
            nameChangeMessageSender.SendCondition += SendNameChangeCondition;
            nameChangeMessageSender.MessageToObserver += SendNameChangeMessage;
            KickStart().Forget();
        }

        public void LateTick()
        {
            ChangeEquipmentOnTimer();
        }

        public void Dispose()
        {
            states[FakePlayerStateEnum.ChangeEquipment].Exit();
            nameChangeMessageSender.SendCondition -= SendNameChangeCondition;
            nameChangeMessageSender.MessageToObserver -= SendNameChangeMessage;
        }

        private void ChangeEquipmentOnTimer()
        {
            changeEquipmentTimer += Time.deltaTime;

            if (changeEquipmentTimer < identity.TimeToChangeEquipment)
                return;

            changeEquipmentTimer = 0;
            states[FakePlayerStateEnum.ChangeEquipment].Perform();
        }

        private bool SendNameChangeCondition()
        {
            return true;
        }

        private ServerMessageToObserver<FakePlayerNameChangeMessage> SendNameChangeMessage()
        {
            var toOtherMessage = new FakePlayerNameChangeMessage(networkIdentity.netId, identity.name);
            return new ServerMessageToObserver<FakePlayerNameChangeMessage>(false, networkIdentity, toOtherMessage);
        }

        private async UniTaskVoid KickStart()
        {
            string name = "";
            string number = "";
            int nameRandom = Random.Range(0, 3);
            int numberRandom = Random.Range(0, 9);

            switch (nameRandom)
            {
                case 0:
                    name = Name.First();
                    break;
                case 1:
                    name = Internet.UserName();
                    break;
                case 2:
                    name = Internet.UserName() + Country.TwoLetterCode();
                    break;
            }

            switch (numberRandom)
            {
                case 6:
                    name += Random.Range(0, 10).ToString("D2");
                    break;
                case 7:
                    name += Random.Range(10, 100).ToString("D2");
                    break;
                case 8:
                    name += Random.Range(100, 1000).ToString("D3");
                    break;
            }

            identity.name = identity.assetId == 248666468 ? name : identity.name;

            foreach (var pair in networkIdentity.observers)
            {
                pair.Value.Send(new FakePlayerNameChangeMessage(networkIdentity.netId, identity.name));
            }

            states[FakePlayerStateEnum.ChangeEquipment].Perform();
            await UniTask.WaitForSeconds(3);
            states[FakePlayerStateEnum.Sanpo].Perform();
        }
    }
}