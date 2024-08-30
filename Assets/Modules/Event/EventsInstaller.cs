using UnityEngine;
using Zenject;

namespace com.playbux.events
{
    [CreateAssetMenu(fileName = "EventsInstaller", menuName = "Playbux/Event/EventsInstaller")]
    public class EventsInstaller : ScriptableObjectInstaller<EventsInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<ConversationDialogSignal>();
            Container.DeclareSignal<AuthenticationSignal>();
            Container.DeclareSignal<QuestRewardSignal>();
            Container.DeclareSignal<IdentityChangeSignal>();
            Container.DeclareSignal<ChangeThisPlayerNameSignal>();
            Container.DeclareSignal<LoginSignal>();
            Container.DeclareSignal<LogoffSignal>();
            Container.DeclareSignal<TeleportationInitiateSignal>();
            Container.DeclareSignal<TeleportationCompleteSignal>();
            Container.DeclareSignal<RefreshIconSignal>();
            Container.DeclareSignal<RemoteConfigFetchRequestSignal>();
            Container.DeclareSignal<RemoteConfigResponseSignal<int>>();
            Container.DeclareSignal<RemoteConfigResponseSignal<float>>();
            Container.DeclareSignal<RemoteConfigResponseSignal<string>>();
            Container.DeclareSignal<CTETopFiveDataSignal>();
            Container.DeclareSignal<BGMPlaySignal>();
            Container.DeclareSignal<BGMStopSignal>();
            Container.DeclareSignal<BGMStopAllSignal>();
            Container.DeclareSignal<SFXPlaySignal>();
            Container.DeclareSignal<QuizTimeEventSignal>();
            Container.DeclareSignal<SettingDataSignal>();
            Container.DeclareSignal<LinkOutSignal>();
            Container.DeclareSignal<NotificationUISignal>();
            Container.DeclareSignal<OnStartClientSignal>();
        }
    }
}