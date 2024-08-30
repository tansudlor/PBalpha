
using com.playbux.networking.mirror.message;
using Mirror;
using UnityEngine;
using Zenject;
using com.playbux.events;

namespace com.playbux.gameevent
{
    public class GameEventClient 
    {

        private INetworkMessageReceiver<MiniEventMessage> miniEventMessage;
        private SignalBus signal;

        [Inject]
        void SetUp(INetworkMessageReceiver<MiniEventMessage> miniEventMessage,SignalBus signalBus)
        {
            this.signal = signalBus;
            this.miniEventMessage = miniEventMessage;
            this.miniEventMessage.OnEventCalled += OnMiniEventRespone;
        }

        public void JoinEvent()
        {
            Debug.Log("joinEvent");
            NetworkClient.Send(new MiniEventMessage("join", NetworkClient.localPlayer.netId.ToString(), new QuestionInfo()));

        }

        public void SendEvent(string quizId)
        {
            Debug.Log("SendEvent");
            NetworkClient.Send(new MiniEventMessage("quiztimeeventstartnow", quizId, new QuestionInfo()));
        }

        private void OnMiniEventRespone(MiniEventMessage miniEventMessage)
        {
#if DEVELOPMENT
            Debug.Log("MiniEvnet Subscription");
#endif
            QuizTimeEventSignal quizTimeEventSignal = new QuizTimeEventSignal();
            quizTimeEventSignal.Command = miniEventMessage.Command;
            quizTimeEventSignal.Message = miniEventMessage.Message;
            quizTimeEventSignal.Data = miniEventMessage.QuestionInfo;
            signal.Fire(quizTimeEventSignal);
            
        }
    }

}

