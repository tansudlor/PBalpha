using TMPro;
using Zenject;
using UnityEngine;
using UnityEngine.UI;

namespace com.playbux.ui.timebar
{
    public class TimerGaugeUI : ITickable
    {
        private readonly Image castFill;
        private readonly GameObject gameObject;

        private bool isTicking;
        private float time;
        private float duration;
        public TimerGaugeUI(GameObject gameObject, Image castFill)
        {
            this.castFill = castFill;
            this.gameObject = gameObject;
        }

        public void Tick()
        {
            if (!isTicking)
                return;

            time += Time.deltaTime;
            float percentage = time / duration;
            castFill.fillAmount = percentage;
        }

        public void Start(float forceStartTime, float duration)
        {
            if (forceStartTime >= duration)
                return;

            float percentage = forceStartTime / duration;
            time = forceStartTime;
            castFill.fillAmount = percentage;
            isTicking = true;
            gameObject.SetActive(isTicking);
        }

        public void Stop()
        {
            isTicking = false;
            gameObject.SetActive(isTicking);
        }

        public class Factory : PlaceholderFactory<TimerGaugeUI, Factory>
        {

        }
    }

    public class CastBarUIInstaller : MonoInstaller<CastBarUIInstaller>
    {
        [SerializeField]
        private Image castFill;

        [SerializeField]
        private TextMeshProUGUI castText;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CastBarUI>().AsSingle();
            Container.Bind<Image>().FromInstance(castFill).AsSingle();
            Container.Bind<GameObject>().FromInstance(gameObject).AsSingle();
            Container.Bind<TextMeshProUGUI>().FromInstance(castText).AsSingle();
        }
    }

    public class CastBarUI : ITickable
    {
        private readonly Image castFill;
        private readonly GameObject gameObject;
        private readonly TextMeshProUGUI castText;

        private bool isTicking;
        private float time;
        private float duration;
        public CastBarUI(GameObject gameObject, Image castFill, TextMeshProUGUI castText)
        {
            this.castFill = castFill;
            this.castText = castText;
            this.gameObject = gameObject;
        }

        public void Tick()
        {
            if (!isTicking)
                return;

            time += Time.deltaTime;
            float percentage = time / duration;
            castFill.fillAmount = percentage;
        }

        public void Start(string castName, float forceStartTime, float duration)
        {
            if (forceStartTime >= duration)
                return;

            castText.text = castName;
            float percentage = forceStartTime / duration;
            time = forceStartTime;
            castFill.fillAmount = percentage;
            isTicking = true;
            gameObject.SetActive(isTicking);
        }

        public void Stop()
        {
            isTicking = false;
            gameObject.SetActive(isTicking);
        }

        public class Factory : PlaceholderFactory<CastBarUI, Factory>
        {

        }
    }
}