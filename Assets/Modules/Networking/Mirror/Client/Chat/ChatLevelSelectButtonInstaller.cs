using TMPro;
using System;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using com.playbux.networking.mirror.core;

namespace com.playbux.networking.mirror.client.chat
{
    [Serializable]
    public struct ChatLevelAndCommand
    {
        public bool enabled;
        public string command;
        public ChatLevel level;
    }

    public class LinePlaceholderFactory : PlaceholderFactory<Object, Transform, Image>
    {

    }

    public class LineFactory : IFactory<Object, Transform, Image>
    {
        private readonly DiContainer container;

        public LineFactory(DiContainer container)
        {
            this.container = container;
        }

        public Image Create(Object prefab, Transform parent)
        {
            return container.InstantiatePrefabForComponent<Image>(prefab, parent);
        }
    }

    [Serializable]
    public struct ChatLevelSelectButtonFacade
    {
        public Image Background => background;

        public Button Button => button;

        public GameObject ChatModeSlot => chatModeSlot;

        public TextMeshProUGUI ModeText => modeText;

        public GameObjectContext PalettePrefab => palettePrefab;

        public GameObject LineGameObject => lineGameObject;

        [SerializeField]
        private Image background;

        [SerializeField]
        private Button button;

        [SerializeField]
        private GameObject chatModeSlot;

        [SerializeField]
        private TextMeshProUGUI modeText;

        [SerializeField]
        private GameObjectContext palettePrefab;

        [SerializeField]
        private GameObject lineGameObject;
    }

    public class ChatLevelSelectButtonInstaller : MonoInstaller<ChatLevelSelectButtonInstaller>
    {
        [Inject]
        private ChatConfig config;

        [SerializeField]
        private ChatLevelSelectButtonFacade facade;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChatLevelSelectButton>().AsSingle();
            Container.Bind<ChatLevelAndCommand[]>().FromInstance(config.Levels).AsSingle();
            Container.Bind<ChatLevelSelectButtonFacade>().FromInstance(facade).AsSingle();
            Container.BindFactory<Object, Transform, Image, LinePlaceholderFactory>().FromFactory<LineFactory>();
            Container.BindFactory<ChatLevelPalette, ChatLevelPalette.Factory>().FromSubContainerResolve().ByNewContextPrefab(facade.PalettePrefab).AsSingle();

            Container.BindSignal<UserChatLevelChangeSignal>().ToMethod<ChatLevelSelectButton>(getter => getter.OnSendLevelChanged).FromResolve();
        }
    }
}