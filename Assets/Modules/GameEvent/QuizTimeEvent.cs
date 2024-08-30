using Mirror;
using System;
using Zenject;
using System.IO;
using System.Linq;
using UnityEngine;
using com.playbux.api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.firebaseservice;
using com.playbux.schedulessetting;
using com.playbux.gameeventcollection;
using com.playbux.networking.mirror.message;

namespace com.playbux.gameevent
{
    public class QuizTimeEvent : BaseGameEvent<JToken>
    {
#if !SERVER
        public override void CloseEvent()
        {

        }

        public override void OnClientConnected(NetworkConnectionToClient connection)
        {

        }

        public override async UniTaskVoid RunEvent()
        {
            
        }

        public override void SetJobDiscription(JToken desc)
        {

        }
#endif

#if SERVER
        [NonSerialized]
        public List<string> participants = new List<string>();

        [NonSerialized]
        public List<Question> questions = new List<Question>();

        private PositionCollection positionCollection;
        private GameEventCollection gameEventCollection;
        private IIdentitySystem identitySystem;
        private IEventScore eventScoreCollector;
        private QuestionGame questGame;
        private IServerNetworkMessageReceiver<MiniEventMessage> miniEventMessageReceiver;
        private GameObject[] areaArray;
        private JToken eventData;
        private string questionId;
        private List<QuizAnswer> quizAnswers;
        private long startTick = 0;
        private int reward = 0;


        [Inject]
        private void SetUp(
            IIdentitySystem identitySystem,
            IEventScore eventScoreCollector,
            PositionCollection positionCollection,
            GameEventCollection gameEventCollection,
            IServerNetworkMessageReceiver<MiniEventMessage> miniEventMessageReceiver)
        {
#if DEVELOPMENT
            Debug.Log("MINIEVENT sub");
#endif
            this.identitySystem = identitySystem;
            this.eventScoreCollector = eventScoreCollector;
            this.miniEventMessageReceiver = miniEventMessageReceiver;
            this.positionCollection = positionCollection;
            this.gameEventCollection = gameEventCollection;
            Debug.Log(this.miniEventMessageReceiver + " MINIEVENT");
            this.miniEventMessageReceiver.OnEventCalled += OnMiniEventMessageReceiver;
        }

        public override void SetJobDiscription(JToken desc)
        {
            eventData = desc;
        }

        public override async UniTaskVoid RunEvent()
        {
#if DEVELOPMENT
            Debug.Log(eventData.ToString());
#endif

            questions.Clear();

            areaArray = new GameObject[3];
            var ring1 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());
            var ring2 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());
            var ring3 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());

            areaArray[0] = ring1;
            areaArray[1] = ring2;
            areaArray[2] = ring3;
            QuestionGame questionGame = new QuestionGame();

            string eventId = eventData["_id"].ToString();
            string quizId = eventData["quiz"].ToString();
            Debug.Log("sessionDate : " + eventData["session_date"].ToObject<DateTime>().Ticks);
            startTick = eventData["session_date"].ToObject<DateTime>().Ticks;
            string data = await APIServerConnector.GetQuizEvent(quizId);
            JObject quizData = JsonConvert.DeserializeObject<JObject>(data);
            string quizName = quizData["name"].ToString();

            questionGame.eventname = quizName;

            Debug.Log(quizData["questions"].Children().Count());

            for (int i = 0; i < quizData["questions"].Children().Count(); i++)
            {
                Question question = new Question();

                JToken currentQuestion = quizData["questions"][i];
                question.choices = new Dictionary<int, JToken>();
                question.question = currentQuestion["desc"].ToString();
                JToken choices = currentQuestion["choices"];

                for (int j = 0; j < choices.Children().Count(); j++)
                {
                    JToken currentChoice = currentQuestion["choices"][j];
                    int key = currentChoice["no"].ToObject<int>();
                    string value = currentChoice["desc"].ToString();
                    question.choices[j + 1] = currentChoice;

                }

                question.correctAnswer = currentQuestion["answer_no"].ToObject<int>();
                question.questionId = currentQuestion["_id"].ToString();
                question.rewards = currentQuestion["rewards"].ToString();
                questions.Add(question);

            }

            await GameEventRun(eventId, questionGame.eventname, quizData["questions"].Children().Count());
        }

        public override void CloseEvent()
        {
            Debug.Log("CloseEvent");
            Destroy(areaArray[0], 0.1f);
            Destroy(areaArray[1], 0.1f);
            Destroy(areaArray[2], 0.1f);
            Array.Clear(areaArray, 0, areaArray.Length);
            questions.Clear();
            participants.Clear();
        }

        private void OnMiniEventMessageReceiver(NetworkConnectionToClient connection, MiniEventMessage message,
            int channel)
        {
#if DEVELOPMENT
            Debug.Log("Server MiniEM received message: " + message);
            Debug.Log("Server MiniEM Find NetworkIdentity");
            Debug.Log("Require MiniEM *************************" + connection);
#endif
            var commad = message.Command;
            if (commad == "join")
            {
                var netIdString = message.Message;
                var netId = uint.Parse(netIdString);
                var uid = identitySystem[netId].UID;
#if PRODUCTION && !UNITY_EDITOR
                /*if (identitySystem[netId].CanPlayQuiz == false)
                {
                    connection.Send(new MiniEventMessage("cantplay", "CantNotPlay", new QuestionInfo()));
                }
                else
                {
                    participants.Add(uid);
                }*/
                participants.Add(uid);
#endif

#if PRODUCTION && UNITY_EDITOR
                participants.Add(uid);
#endif

#if !PRODUCTION
                participants.Add(uid);
#endif

                Debug.Log("Succes " + uid);
            }
        }

        private async UniTask GameEventRun(string eventId, string eventName, int questionCount)
        {
            participants.Clear();

            gameEventCollection.Assign(this, GameEventState.None);
            long remainTick = APIServerConnector.SecondToTicks(60);
            NetworkServer.SendToReady(new MiniEventMessage("eventannouncement", startTick + "," + remainTick,
                new QuestionInfo())); //this will trigger join event button
#if DEVELOPMENT
            Debug.Log("eventannouncement");
#endif
            gameEventCollection.Assign(this, GameEventState.Prepare);
            int waitSec = ScheduleSetting.WaitSec;
            endPrepare = APIServerConnector.SecondToTicks(waitSec);
            await UniTask.Delay(waitSec * 1000);
            SendToParticipants(new MiniEventMessage("begin", eventName + " will start in 5 sec", new QuestionInfo()));

#if DEVELOPMENT
            Debug.Log("begin");
#endif
            await UniTask.Delay(5000);
            gameEventCollection.Assign(this, GameEventState.Play);
            SendToParticipants(new MiniEventMessage("play", eventName + " start NOW!!!!", new QuestionInfo()));

#if DEVELOPMENT
            Debug.Log("play");
#endif
            //send only participate

            //sequence each question to npc
            for (int i = 0; i < questionCount; i++)
            {
                //Random Position 
                string[] select = { "0", "1", "2", "3", "4", "5" };

                for (int j = 0; j < 10; j++)
                {
                    int r1 = UnityEngine.Random.Range(0, select.Length);
                    int r2 = UnityEngine.Random.Range(0, select.Length);
                    (select[r1], select[r2]) = (select[r2], select[r1]);
                }


                Debug.Log("[GetQuizEvent] : " + JsonConvert.SerializeObject(select));


                int answerIndex = questions[i].correctAnswer - 1;

                Debug.Log("[GetQuizEvent] : " + answerIndex + " answerIndex");


                string[] color = { "Blue is Correct", "Yellow is OK", "Other is Wrong" };

                for (int j = 0; j < 3; j++)
                {
                    var areaRing = areaArray[j];
                    areaRing.transform.position = positionCollection.Positoions[int.Parse(select[j])];
                    areaRing.GetComponent<SpriteRenderer>().color = positionCollection.Colors[j];
                    if (!questions[i].choices.ContainsKey(j + 1))
                    {
                        questions[i].choices[j + 1] = color[j];
                    }
                }

                string[] choices;
                try
                {
                    choices = new string[]
                    {
                        questions[i].choices[1]["desc"].ToString(),
                        questions[i].choices[2]["desc"].ToString(),
                        questions[i].choices[3]["desc"].ToString(),
                    };
                }
                catch (Exception ex)
                {
                    choices = new string[]
                    {
                        "Junior",
                        "Data Editor",
                        "PlayBux Alpha",
                    };

                    Debug.Log("[GetQuizEvent] : " + JsonConvert.SerializeObject(questions[i]));
                }

                QuestionInfo message = new QuestionInfo(questions[i].question, choices);

                Debug.Log("[GetQuizEvent] : " + message);

#if DEVELOPMENT
                SendToParticipants(new MiniEventMessage("ingame", string.Join("", select), message));
                Debug.Log("ingame");
#endif

#if !DEVELOPMENT
                SendToParticipants(new MiniEventMessage("ingame", string.Join("", select), message));
#endif
                //wait for capture position
                await UniTask.Delay(5000);
                SendToParticipants(new MiniEventMessage("preparecapture", DateTime.UtcNow.Ticks.ToString(),
                    new QuestionInfo()));
#if DEVELOPMENT
                Debug.Log("preparecapture");
#endif
                await UniTask.Delay(5000);

                for (int j = 1; j < questions[i].choices.Count() + 1; j++)
                {
                    if (questions[i].choices[j]["is_correct"].ToObject<bool>())
                    {
                        answerIndex = j - 1;
                    }
                }

                quizAnswers = new List<QuizAnswer>();
                for (int j = participants.Count - 1; j >= 0; j--)
                {
                    try // protect when participants disconnected  , uid was lost for identity system while OnPlayDisconnect  
                    {
                        QuizAnswer answer = new QuizAnswer();
                        uint score = 0;
                        string uid = participants[j];
                        uint netId = identitySystem.NameReverse[participants[j]];
                        Vector2 playerPos = identitySystem[netId].Identity.gameObject.transform.position;
                        Vector2 correctAnswer = areaArray[answerIndex].transform.position;
                        int answerSelect = -1;

                        for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                        {
                            Vector2 currentChoice = areaArray[choiceIndex].transform.position;

                            if ((playerPos - currentChoice).magnitude < 5f)
                            {
                                answerSelect = choiceIndex;

                            }
                        }

                        Debug.Log("[GetQuizEvent] : " + answerSelect + " ************** " + answerIndex);

                        if (answerSelect == answerIndex) //win
                        {
                            score = await eventScoreCollector.GetScore(uid, eventName);
                            await eventScoreCollector.SendScore(uid, eventName, score + 1);
                            identitySystem[netId].Identity.connectionToClient
                                .Send(new MiniEventMessage("youwin", questions[i].rewards, new QuestionInfo()));
                            answer.user = identitySystem[netId].UID;
                            answer.answer = questions[i].correctAnswer;
                            quizAnswers.Add(answer);
                        }
                        else if (answerSelect != -1) //fail
                        {
                            identitySystem[netId].Identity.connectionToClient
                                .Send(new MiniEventMessage("wrong", "wrong", new QuestionInfo()));

                            Debug.Log("[GetQuizEvent] : userIDanswerSelect " + answerSelect);
                            answer.user = identitySystem[netId].UID;
                            var wrongAnswer = questions[i].correctAnswer + 1;
                            if (wrongAnswer > 3)
                            {
                                wrongAnswer = questions[i].correctAnswer - 1;
                            }
                            answer.answer = wrongAnswer;
                            quizAnswers.Add(answer);
                        }
                        else //miss
                        {
                            identitySystem[netId].Identity.connectionToClient
                                .Send(new MiniEventMessage("miss", "miss", new QuestionInfo()));
                        }

                    }
                    catch(KeyNotFoundException ex)
                    {
                        Debug.Log("[QuizTime] :Expected catch when user was Disconnect , " + DateTime.UtcNow.ToString());
                        Debug.Log(DateTime.UtcNow.ToString() + "" +ex.Message);
                        participants.RemoveAt(j);
                    }
                    catch(Exception ex)
                    {
                        Debug.Log("[QuizTime] :Unexpected catch unknown case ," + DateTime.UtcNow.ToString());
                        Debug.Log(DateTime.UtcNow.ToString() + "" + ex.Message);
                        participants.RemoveAt(j);
                    }

                }

                Debug.Log("[GetQuizEvent] : " + quizAnswers.Count);

                await UniTask.Delay(3000);

                SendToParticipants(new MiniEventMessage("nextquestion", eventName + " next question",
                    new QuestionInfo()));

                Debug.Log("[GetQuizEvent] : next Quiz");

                bool isSend = false;
                string report = "";
                string body = "";
                string path = "";

                if (quizAnswers.Count > 0)
                {
                    (isSend, report, body, path) = await APIServerConnector.QuizAnswerAPI(eventId, questions[i].questionId, quizAnswers);
                    if (isSend == false)
                    {
                        APIRecovery.GetInstante().ReportAPIFailData(path, body, report, false);
                        APIRecovery.GetInstante().Save();

                    }
                }

                quizAnswers.Clear();
            }

            gameEventCollection.Assign(this, GameEventState.End);
            gameEventCollection.Clean();

            SendToParticipants(new MiniEventMessage("end", eventName + " Event End", new QuestionInfo()));
#if DEVELOPMENT
            Debug.Log("end");
#endif
            NetworkServer.SendToReady(new MiniEventMessage("closeevent", eventName + " close", new QuestionInfo()));

            CloseEvent();
            //APIRecovery.GetInstante().SaveToFirebase();

        }

        private void SendToParticipants(MiniEventMessage message)
        {
            for (int j = participants.Count - 1; j >= 0; j--)
            {

                try // protect when participants disconnected  , uid was lost for identity system while OnPlayDisconnect  
                {
                    uint netId = identitySystem.NameReverse[participants[j]]; //throw when lost key in id-system 
                    identitySystem[netId].Identity.connectionToClient.Send(message); //throw when lost networkIdentity  
                }
                catch
                {
                    participants.RemoveAt(j);
                }
            }
        }

        public override void OnClientConnected(NetworkConnectionToClient connection)
        {
            if (gameEventCollection.GetState(this) != GameEventState.Prepare)
                return;

            long remainTick = startTick + APIServerConnector.SecondToTicks(60) - DateTime.UtcNow.Ticks;
            connection.Send(new MiniEventMessage("eventannouncement",
                startTick.ToString() + "," + remainTick.ToString(), new QuestionInfo()));
        }

#endif
    }
}




