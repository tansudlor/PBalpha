using TMPro;
using Zenject;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace com.playbux.networking.mirror.client.chat
{
    [Serializable]
    public class ChalLevelPaletteFacade
    {
        public GameObject Line => line;
        public Button Button => button;
        public Transform Transform => transform;

        public TextMeshProUGUI NameText => nameText;
        
        public TextMeshProUGUI CommandText => commandText;

        [SerializeField]
        private Button button;

        [SerializeField]
        private GameObject line;
        
        [SerializeField]
        private Transform transform;

        [SerializeField]
        private TextMeshProUGUI nameText;

        [SerializeField]
        private TextMeshProUGUI commandText;

    }

    public class ChatLevelPaletteInstaller : MonoInstaller<ChatLevelPaletteInstaller>
    {
        [SerializeField]
        private ChalLevelPaletteFacade dataFacade;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChatLevelPalette>().AsSingle();
            Container.Bind<ChalLevelPaletteFacade>().FromInstance(dataFacade).AsSingle();
        }
    }
}