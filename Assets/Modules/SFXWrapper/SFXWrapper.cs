using com.playbux.events;
using Zenject;

namespace com.playbux.sfxwrapper
{
    public class SFXWrapper
    {
        private static SFXWrapper instance;
        public bool DisableMode;
        public int Profile = 1;

        private SignalBus signalBus;
        public SFXWrapper(SignalBus signalBus)
        {
            this.signalBus = signalBus;
            instance = this;
            DisableMode = true;
        }

        public static SFXWrapper getInstance()
        {
            return instance;
        }

        public void PlaySFX(string path)
        {
            if (DisableMode)
                return;
            signalBus.Fire(new SFXPlaySignal(path + "/" + Profile));
        }
    }
}
