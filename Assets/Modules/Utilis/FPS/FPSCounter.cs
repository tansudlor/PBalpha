using Zenject;
using UnityEngine;
using com.playbux.utilis.canvas;
using com.playbux.utilis.uitext;

namespace com.playbux.utilis.fps
{
    public class FPSCounter : ITickable
    {
        private readonly DebugCanvas canvas;
        private readonly UITextInformation.Factory textFactory;

        private int fps;
        private int frameCount;
        private float timer;
        private UITextInformation fpsText;
        private FPSCounterEnabled counterEnabled;

        public void FPSCounterEnableSignal(FPSCounterEnabled signal)
        {
            counterEnabled = counterEnabled.Equals(signal) ? counterEnabled : signal;

            if (counterEnabled.IsEnabled)
            {
                fpsText ??= textFactory.Create();

            }
        }

        public void Tick()
        {
            if (counterEnabled.Equals(false))
                return;

            frameCount++;
            timer += Time.deltaTime;

            if (!(timer >= 1.0f))
                return;

            fps = frameCount;
            frameCount = 0;
            timer -= 1.0f;
            fpsText.SetText(fps.ToString());
        }
    }

}