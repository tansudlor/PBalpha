using TMPro;
using System;
using Mirror;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using com.playbux.api;
using Newtonsoft.Json;
using com.playbux.npc;
using com.playbux.flag;
using System.Collections;
using com.playbux.events;
using com.playbux.minimap;
using com.playbux.identity;
using com.playbux.analytic;
using com.playbux.ui.bubble;
using com.playbux.gameevent;
using com.playbux.sfxwrapper;
using com.playbux.networkquest;
using System.Collections.Generic;
using com.playbux.gameeventcollection;
using com.playbux.networking.mirror.message;

namespace com.playbux.ui.gamemenu
{
    public class QuizTimeEventUI : MonoBehaviour
    {
#if !SERVER
        [SerializeField]
        private Button joinEvent;
        [SerializeField]
        private GameObject[] areaArray;
        [SerializeField]
        private Button sendEvent;
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private Sprite joinedSprite;
        [SerializeField]
        private Sprite joinSprite;
        [SerializeField]
        private Image joinedButton;
        [SerializeField]
        private GameObject eventJoinUI;
        [SerializeField]
        private Image circleTimer;
        [SerializeField]
        private TextMeshProUGUI timerCount;
        [SerializeField]
        private Sprite quizEventSprite;
        [SerializeField]
        private GameObject timerCounterController;
        [SerializeField]
        private Animator joinAnimator;
        [SerializeField]
        private GameObject cantPlayDialog;

        private QuizTimeBanner quizTimeBanner;
        private DialogController dialogController;
        private GameEventClient gameEventClient;
        private SignalBus signalBus;
        private PositionCollection positionCollection;
        private HowToPlayQuizEvent howToPlayQuizEvent;
        private QuestHelperWindow questHelperWindow;
        private NPCDialogController npcDialogController;
        private IFlagCollection<string> flagCollection;
        private NPCDisplayer nPCDisplayer;
        private IIdentitySystem identitySystem;
        private long startTicks = -1;
        private int questionCount = 0;
        private long tickCount = 0;
        private long serverTicks = 0;
        private bool isEventStart = false;

        [Inject]
        private void SetUp(
            SignalBus signalBus,
            NPCDisplayer nPCDisplayer,
            QuizTimeBanner quizTimeBanner,
            IIdentitySystem identitySystem,
            GameEventClient gameEventClient,
            DialogController dialogController,
            QuestHelperWindow questHelperWindow,
            PositionCollection positionCollection,
            HowToPlayQuizEvent howToPlayQuizEvent,
            IFlagCollection<string> flagCollection,
            NPCDialogController npcDialogController,
            NPCDialogController nPCDialogController)
        {
            this.signalBus = signalBus;
            this.nPCDisplayer = nPCDisplayer;
            this.identitySystem = identitySystem;
            this.quizTimeBanner = quizTimeBanner;
            this.flagCollection = flagCollection;
            this.gameEventClient = gameEventClient;
            this.dialogController = dialogController;
            this.questHelperWindow = questHelperWindow;
            this.positionCollection = positionCollection;
            this.howToPlayQuizEvent = howToPlayQuizEvent;
            this.npcDialogController = npcDialogController;
            this.signalBus.Subscribe<QuizTimeEventSignal>(OnQuizTimeEventSignalRecieve);
        }

        public long CurrentServerTime()
        {
            return serverTicks + tickCount;
        }

        public void OnQuizTimeEventSignalRecieve(QuizTimeEventSignal signal)
        {
            Debug.Log("why");
            string command = signal.Command;
            string message = signal.Message;
            if (command == "eventannouncement")
            {
                string ticks = message.Split(",")[0];
                string remainTicks = message.Split(",")[1];
                Debug.Log(ticks + " ticks");
                serverTicks = long.Parse(ticks);
                tickCount = 0;
                startTicks = long.Parse(ticks) + long.Parse(remainTicks);
                StartCoroutine(StopUpdate());

                quizTimeBanner.SetBanner(command, (APIServerConnector.TicksToSecond(startTicks - CurrentServerTime()).ToString()));
                questHelperWindow.CloseQuestHelper();

                StartCoroutine(ShowTutorial());
                ShowNewEvent();

                StartCoroutine(ShowNPCAndIcon());
            }

            else if (command == "begin")
            {
                try
                {
                    DateTime dateTimeFromTick = new DateTime(serverTicks);
                    AnalyticWrapper.getInstance().Log("join_quiz",
                    new LogParameter("user_id", TokenUtility._id)
                    , new LogParameter("round_time", dateTimeFromTick.ToString())
                    );
                }
                catch { }
                quizTimeBanner.SetBanner(command);
                isEventStart = true;
            }

            else if (command == "ingame")
            {

                QuestionInfo questinfo = new QuestionInfo();
                questinfo = (QuestionInfo)signal.Data;
#if DEVELOPMENT
                Debug.Log(JsonConvert.SerializeObject(questinfo));
#endif
                questionCount++;
                var randomNum = message;
#if DEVELOPMENT
                dialogController.DialogData(quizEventSprite, "Question " + questionCount + "/5", questinfo.question, questinfo.choice);

                for (int i = 0; i < areaArray.Length; i++)
                {
                    areaArray[i].SetActive(true);
                }

                for (int i = 0; i < 3; i++)
                {
                    int index = int.Parse(randomNum[i] + "");
                    areaArray[i].transform.position = positionCollection.Positoions[index];
                    areaArray[i].GetComponent<SpriteRenderer>().color = positionCollection.Colors[i];

                }
#endif

#if !DEVELOPMENT
                dialogController.DialogData(quizEventSprite, "Question " + questionCount + "/5", questinfo.question, questinfo.choice);

                for (int i = 0; i < areaArray.Length; i++)
                {
                    areaArray[i].SetActive(true);
                }

                for (int i = 0; i < 3; i++)
                {
                    int index = int.Parse(randomNum[i] + "");
                    areaArray[i].transform.position = positionCollection.Positoions[index];
                    areaArray[i].GetComponent<SpriteRenderer>().color = positionCollection.Colors[i];

                }
#endif
                return;
            }
            else if (command == "end")
            {
                Debug.Log(message);
                isEventStart = false;
                quizTimeBanner.SetBanner(command);
                questionCount = 0;
                CloseEvent();
                return;
            }
            else if (command == "play")
            {
                Debug.Log(message);
                areaArray = new GameObject[3];
                var ring1 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());
                var ring2 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());
                var ring3 = Instantiate(positionCollection.AreaPrefab, Vector3.one * 10000, new Quaternion());

                areaArray[0] = ring1;
                areaArray[1] = ring2;
                areaArray[2] = ring3;

                for (int i = 0; i < areaArray.Length; i++)
                {
                    areaArray[i].SetActive(false);
                }

                return;
            }
            else if (command == "preparecapture")
            {
                Debug.Log(message);
                string ticks = message;
                long waitTime = 5 * 10_000_000;//long.Parse(ticks) + APIServerConnector.SecondToTicks(5);
                timerCounterController.SetActive(true);
                timerCounterController.GetComponent<TimerCounterController>().SetCapture(waitTime);
                timerCounterController.GetComponent<TimerCounterController>().QuizTimeEventUI = this;
                return;
            }
            else if (command == "youwin")
            {
                dialogController.ClearData();

                //identitySystem[NetworkClient.localPlayer.netId].BalancePebble += 1;

                List<Reward> rewards = JsonConvert.DeserializeObject<List<Reward>>(message);

                quizTimeBanner.SetBanner(command, rewards);
                for (int i = 0; i < rewards.Count; i++)
                {

                    try
                    {
                        string property = IdentityDetail.VariableCoverter[rewards[i].type];
                        int readProperty = (int)identitySystem[NetworkClient.localPlayer.netId].GetType().GetProperty(property).GetValue(identitySystem[NetworkClient.localPlayer.netId]);
                        identitySystem[NetworkClient.localPlayer.netId].GetType().GetProperty(property).SetValue(identitySystem[NetworkClient.localPlayer.netId], readProperty + rewards[i].quantity);
                    }
                    catch (Exception)
                    {
#if DEVELOPMENT
                        Debug.LogWarning("IdentityDetail can't convert type " + rewards[i].type);
#endif
                    }
                }
                try
                {
                    DateTime dateTimeFromTick = new DateTime(serverTicks);
                    AnalyticWrapper.getInstance().Log("quiz_completed",
                    new LogParameter("user_id", TokenUtility._id)
                    , new LogParameter("round_time", dateTimeFromTick.ToString())
                       , new LogParameter("pebble_type", "normal")
                         , new LogParameter("total_pebble", rewards[0].quantity.ToString())
                    );
                }
                catch
                {

                }

                Debug.Log(message);
                return;
            }
            else if (command == "wrong")
            {
                dialogController.ClearData();
                quizTimeBanner.SetBanner(command);

                Debug.Log(message);
                return;
            }

            else if (command == "miss")
            {
                dialogController.ClearData();
                quizTimeBanner.SetBanner(command);
                Debug.Log(message);
                return;
            }

            else if (command == "nextquestion")
            {
                Debug.Log(message);
                quizTimeBanner.CloseBanner();
                for (int i = 0; i < areaArray.Length; i++)
                {
                    areaArray[i].SetActive(false);
                }
                return;
            }

            else if (command == "quiztimeeventstartnow")
            {
                Debug.Log("quiztimeeventstartnow");
                Debug.Log(message);
                return;
            }


            else if (command == "closeevent")
            {
                Debug.Log(message);
                try //npc quiz event only
                {
                    CloseNPC();
                }
                catch
                {

                }
                return;
            }

            else if (command == "cantplay")
            {
                Debug.Log(message);
                cantPlayDialog.transform.SetParent(gameObject.transform);
                var currentParent = gameObject.transform.parent;
                cantPlayDialog.transform.SetParent(currentParent);
                cantPlayDialog.SetActive(true);
                joinedButton.sprite = joinSprite;
                joinEvent.interactable = true;
                return;
            }
        }


        private IEnumerator ShowNPCAndIcon()
        {
            yield return new WaitUntil(() => NetworkClient.localPlayer != null);
            yield return new WaitUntil(() => NetworkClient.localPlayer.netId != 0);
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId] != null);
            yield return new WaitUntil(() => identitySystem[NetworkClient.localPlayer.netId].UID != null);
            yield return new WaitUntil(() => flagCollection.GetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, "N4") != null);
            flagCollection.Remove(identitySystem[NetworkClient.localPlayer.netId].UID, "N4");
            nPCDisplayer.DisplayAvalibleNPC();
            MiniMapLocator.CloseIcon.Remove("4");
            MiniMapLocator.RefreshIconNow = true;
            MiniMapLocator.PlayAnim.Add("4");


        }
        private IEnumerator ShowTutorial()
        {
            yield return new WaitForSeconds(3f);
            SFXWrapper.getInstance().PlaySFX("SFX/MessageBox");
            howToPlayQuizEvent.ShowHowToPlay();
        }

        public void JoinEventButton()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            joinedButton.sprite = joinedSprite;
            joinEvent.interactable = false;
            gameEventClient.JoinEvent();

        }

        public void CloseTimer()
        {
            SFXWrapper.getInstance().PlaySFX("SFX/Click");
            eventJoinUI.SetActive(false);
        }

        public void ShowNewEvent()
        {
            eventJoinUI.SetActive(true);
            joinAnimator.Play("QuizJoinTimmerAnim");
            joinedButton.sprite = joinSprite;
            joinEvent.interactable = true;

        }

        private void CloseEvent()
        {
            Debug.Log("CloseEvent");
            Destroy(areaArray[0], 0.1f);
            Destroy(areaArray[1], 0.1f);
            Destroy(areaArray[2], 0.1f);
            Array.Clear(areaArray, 0, areaArray.Length);
            dialogController.ClearData();

        }

        private void CloseNPC()
        {
            flagCollection.SetFlag(identitySystem[NetworkClient.localPlayer.netId].UID, "N4", "Mr. Curious", "0");
            nPCDisplayer.DisplayAvalibleNPC();
            MiniMapLocator.CloseIcon.Add("4");
            MiniMapLocator.PlayAnim.Remove("4");
            MiniMapLocator.RefreshIconNow = true;
        }

        public void SendEvent()
        {
            var quizID = inputField.text;
            gameEventClient.SendEvent(quizID);
        }



        private IEnumerator StopUpdate()
        {
            yield return new WaitUntil(() => startTicks - CurrentServerTime() < 0);
            eventJoinUI.SetActive(false);
            howToPlayQuizEvent.CloseHowToPlay();

        }

        private void Start()
        {

            CloseTimer();
        }

        private void Update()
        {
            tickCount += (long)(Time.deltaTime * 10000000);

            if (isEventStart)
            {
                npcDialogController.HideDialog();
            }


            if (startTicks < 0)
            {
                return;
            }

            long delta = startTicks - CurrentServerTime();
            int displayTime = APIServerConnector.TicksToSecond(delta);
            if (displayTime > 60)
            {

                displayTime = 60;
            }


            if (displayTime < 0)
            {
                displayTime = 0;
            }

            timerCount.text = displayTime.ToString();
            circleTimer.fillAmount = (displayTime / 60f) * 1.00f;
        }
#endif
    }
}