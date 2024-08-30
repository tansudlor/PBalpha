using TMPro;
using Zenject;
using UnityEngine;
using com.playbux.events;

namespace com.playbux.networking.mirror.client.building
{
    public class CTETextPrefabBuilding : PrefabBuilding
    {
        [SerializeField]
        private TextMeshProUGUI[] rankTexts;
        
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
            for (int i = 0; i < rankTexts.Length; i++)
            {
                rankTexts[i].text = "";
                rankTexts[i].text += $"{signal.ranks[i]}-{signal.countryNames[i]}";
                rankTexts[i].text += "                        ";
                rankTexts[i].text += $"{signal.totalPixels[i]} pixels";
                rankTexts[i].gameObject.SetActive(true);
            }
        }
    }
}