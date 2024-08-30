using System;
using Zenject;
using com.playbux.networking.mirror.core;
using com.playbux.networking.mirror.message;
using Random = UnityEngine.Random;

namespace com.playbux.networking.mirror.server.fakeplayer
{
    public class RandomEquipmentFakePlayerState : IFakePlayerState, ILateDisposable
    {
        public FakePlayerStateEnum StateEnum { get; }

        private readonly FakePlayerIdentity fakePlayerIdentity;
        private readonly IServerMessageSender<FakePlayerPartMessage> partSender;

        private bool isPerforming;
        private string[] partValues;

        public RandomEquipmentFakePlayerState(
            FakePlayerStateEnum stateEnum,
            FakePlayerIdentity fakePlayerIdentity,
            IServerMessageSender<FakePlayerPartMessage> partSender)
        {
            StateEnum = stateEnum;
            this.partSender = partSender;
            this.fakePlayerIdentity = fakePlayerIdentity;
            this.partSender.MessageToObserver += SendPartMessage;
            this.partSender.SendCondition += SendPartMessageCondition;
            partValues = new string[7];
        }

        public void LateDispose()
        {
            partSender.MessageToObserver -= SendPartMessage;
            partSender.SendCondition -= SendPartMessageCondition;
        }

        public void Perform()
        {
            isPerforming = true;
            ChangePart();
            Exit();
        }
        public void Exit()
        {
            isPerforming = false;
        }

        private void ChangePart()
        {
            int index = Random.Range(0, fakePlayerIdentity.HatCollection.ids.Length);
            partValues[0] = "1_" + fakePlayerIdentity.HatCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.FaceCollection.ids.Length);
            partValues[1] = "1_" + fakePlayerIdentity.FaceCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.HeadCollection.ids.Length);
            partValues[2] = "1_" + fakePlayerIdentity.HeadCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.ShirtCollection.ids.Length);
            partValues[3] = "1_" + fakePlayerIdentity.ShirtCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.PantsCollection.ids.Length);
            partValues[4] = "1_" + fakePlayerIdentity.PantsCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.ShoesCollection.ids.Length);
            partValues[5] = "1_" + fakePlayerIdentity.ShoesCollection.ids[index];
            index = Random.Range(0, fakePlayerIdentity.BackCollection.ids.Length);
            partValues[6] = "1_" + fakePlayerIdentity.BackCollection.ids[index];
        }

        private bool SendPartMessageCondition()
        {
            return true;
        }

        private ServerMessageToObserver<FakePlayerPartMessage> SendPartMessage()
        {
            var message = new FakePlayerPartMessage(fakePlayerIdentity.NetworkIdentity.netId, partValues);
            return new ServerMessageToObserver<FakePlayerPartMessage>(false, fakePlayerIdentity.NetworkIdentity, message);
        }
    }
}