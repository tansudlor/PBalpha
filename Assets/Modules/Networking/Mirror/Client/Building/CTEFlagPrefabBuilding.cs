using System;
using com.playbux.events;
using UnityEngine;
using Zenject;

namespace com.playbux.networking.mirror.client.building
{
    public class CTEFlagPrefabBuilding : PrefabBuilding
    {
        [SerializeField]
        private Renderer referenceRenderer;

        [SerializeField]
        private CTEFlagDatabase flagDatabase;

        [SerializeField]
        private SpriteRenderer[] flagRenderers;

        private SignalBus signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            this.signalBus.Subscribe<CTETopFiveDataSignal>(OnCTERankDataReceived);
        }

        private void OnDestroy()
        {
            signalBus.Unsubscribe<CTETopFiveDataSignal>(OnCTERankDataReceived);
        }

        private void OnCTERankDataReceived(CTETopFiveDataSignal signal)
        {
            if (signal.countryNames?.Length <= 0)
                return;

            for (int i = 0; i < 3; i++)
            {
                var texture = flagDatabase.Get(signal.countryNames[i]);

                if (texture == null)
                    continue;

                flagRenderers[i].sortingOrder = referenceRenderer == null ? 76 : referenceRenderer.sortingOrder + 1;
                flagRenderers[i].sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100, 0, SpriteMeshType.FullRect);
                flagRenderers[i].gameObject.SetActive(true);
            }
        }
    }
}