
using com.playbux.events;
using com.playbux.identity;
using com.playbux.minimap;
using UnityEngine;
using Zenject;

namespace com.playbux.ui.gamemenu
{
    [CreateAssetMenu(menuName = "Playbux/UI/GameUIInstaller", fileName = "GameUIInstaller")]
    public class GameUIInstaller : ScriptableObjectInstaller<GameUIInstaller>
    {
        public GameObject PlayerUIMenuPrefab;
        public GameObject SettingUIPrefab;
        public GameObject ChangeNamePrefab;
        public GameObject LoginPrefab;
        public GameObject LoginFromWebPrefab;
        public GameObject DialogPrefab;
        public GameObject BannerPrefab;
        public GameObject JoinEventPrefab;
        public GameObject HowToPlayQuizGamePrefab;
        public GameObject EntranceBulidingPrefab;
        public GameObject OpenAnimPrefab;
        public GameObject FeatureDialogPrefab;
        public GameObject NotificationDialogPrefab;

        private UICanvas canvas;
        [Inject]
        void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }
        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
#endif
        }
#if !SERVER
        private void BindClientSide()
        {

            Container.Bind<IGameMenuUiController>()
                .To<GameMenuUiController>()
                .FromComponentInNewPrefab(PlayerUIMenuPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle()
                .NonLazy();

            Container.Bind<ISettingUIController>()
               .To<SettingUIController>()
               .FromComponentInNewPrefab(SettingUIPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle()
               .NonLazy();

                Container.Bind<IChangeNameUIController>()
                    .FromSubContainerResolve()
                    .ByNewContextPrefab(ChangeNamePrefab)
                    .UnderTransform(canvas.transform)
                    .AsSingle()
                    .NonLazy();

            Container.Bind<DialogController>()
               .FromComponentInNewPrefab(DialogPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle();

            Container.Bind<QuizTimeBanner>()
               .FromComponentInNewPrefab(BannerPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle();

            Container.Bind<QuizTimeEventUI>()
               .FromSubContainerResolve()
               .ByNewContextPrefab(JoinEventPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle().NonLazy();


            Container.Bind<HowToPlayQuizEvent>()
               .FromComponentInNewPrefab(HowToPlayQuizGamePrefab)
               .UnderTransform(canvas.transform)
               .AsSingle();

            Container.Bind<EntranceBulidingUIController>()
                .FromComponentInNewPrefab(EntranceBulidingPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle().NonLazy();

            Container.Bind<DialogTempleteController>()
               .FromComponentInNewPrefab(FeatureDialogPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle();



#if !LOGIN_BYPASS
            Container.Bind<OpenAnimation>()
               .FromComponentInNewPrefab(OpenAnimPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle().NonLazy();
#endif

#if LOGIN_BYPASS
            Container.Bind<LogInPage>()
                .FromComponentInNewPrefab(LoginPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle();

            Container.BindSignal<AuthenticationSignal>()
                 .ToMethod<LogInPage>(c => c.OnConnectedSignalReceive).FromResolve();
#endif
#if !LOGIN_BYPASS
            Container.Bind<LogInFromWeb>()
               .FromComponentInNewPrefab(LoginFromWebPrefab)
               .UnderTransform(canvas.transform)
               .AsSingle();

            Container.Bind<NotificationUIController>()
              .FromComponentInNewPrefab(NotificationDialogPrefab)
              .UnderTransform(canvas.transform)
              .AsSingle().NonLazy();

            Container.BindSignal<AuthenticationSignal>()
                 .ToMethod<LogInFromWeb>(c => c.OnConnectedSignalReceive).FromResolve();
#endif
        }
#endif
    }
}