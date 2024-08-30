using Zenject;
using UnityEngine;
using com.playbux.ui;
using com.playbux.api;
using com.playbux.flag;
using com.playbux.quest;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using com.playbux.firebaseservice;
using com.playbux.networking.mirror.message;

namespace com.playbux.networkquest
{
    [CreateAssetMenu(menuName = "Playbux/Quest/QuestInstaller", fileName = "QuestInstaller")]
    public class QuestInstaller : ScriptableObjectInstaller<QuestInstaller>
    {
        public GameObject npcDialogPrefab;
        public GameObject questHelperPrefab;

        private UICanvas canvas;

        [Inject]
        private void Setup(UICanvas canvas)
        {
            this.canvas = canvas;

        }
        public override void InstallBindings()
        {
#if !SERVER
            BindClientSide();
            Container.Bind<IQuestRunner>().To<QuestRunner>().AsSingle().NonLazy();

#endif

#if SERVER
            BindServerSide();
            Container.Bind<IQuestRunner>().To<QuestRunner>().AsSingle().NonLazy();
#endif


        }
#if SERVER
        private void BindServerSide()
        {
            FirebaseAuthenticationService.GetInstance().FetchToken().ContinueWith(() => { APIServerConnector.SetAPIDynamicPath(); }).Forget();
            APIRecovery.GetInstante().LoadRecovery();


            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(IServerNetworkMessageReceiver<QuestMessage>))
                .To<ServerNetworkMessageReceiver<QuestMessage>>().AsSingle();
        }
#endif

#if !SERVER
        private void BindClientSide()
        {
            Container.Bind(
                    typeof(IInitializable),
                    typeof(ILateDisposable),
                    typeof(INetworkMessageReceiver<QuestMessage>))
                .To<ClientMessageReceiver<QuestMessage>>().AsSingle();

            /*Container.Bind<NPCDialogController>()
                .FromComponentInNewPrefab(npcDialogPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle()
                .NonLazy();*/

            Container.Bind<NPCDialogController>().FromSubContainerResolve().ByNewContextPrefab(npcDialogPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle().NonLazy();

            Container.Bind<QuestHelperWindow>()
                .FromSubContainerResolve()
                .ByNewContextPrefab(questHelperPrefab)
                .UnderTransform(canvas.transform)
                .AsSingle()
                .NonLazy();


        }
#endif

    }
}