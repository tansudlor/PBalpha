using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace SETHD.Echo
{
    public class CrossFadeBGMChannel : IAudioChannel
    {
        private bool isPaused;
        private string currentKey;
        private readonly List<UniTask> crossFadeTasks;

        private readonly AudioBank audioBank;
        private readonly Dictionary<string, AudioSource> actives;
        private readonly AudioSourceProvider audioSourceProvider;

        public CrossFadeBGMChannel(AudioBank audioBank, AudioSourceProvider audioSourceProvider)
        {
            currentKey = "";
            this.audioBank = audioBank;
            crossFadeTasks = new List<UniTask>();
            actives = new Dictionary<string, AudioSource>();
            this.audioSourceProvider = audioSourceProvider;
            Reinitialize();
        }

        public void Reinitialize()
        {
            foreach (var pair in actives)
            {
                audioSourceProvider.Return(pair.Value);
            }

            actives.Clear();
        }

        public async UniTask Play(string key, PlayMode playMode)
        {
            if (currentKey == key)
                return;

            if (playMode == PlayMode.StartOver)
                Stop().Forget();

            await CrossFade(key, playMode);
        }

        public async UniTask Pause()
        {
            foreach (var pair in actives)
            {
                if (isPaused)
                {
                    pair.Value.UnPause();
                    return;
                }

                pair.Value.Pause();
            }

            isPaused = !isPaused;
            await UniTask.Yield();
        }

        public async UniTask Stop()
        {
            foreach (var pair in actives)
            {
                pair.Value.Stop();
            }

            await UniTask.Yield();
        }

        public async UniTask Stop(string key)
        {
            crossFadeTasks.Add(FadeDownTask(currentKey, 0f));
            await UniTask.WhenAll(crossFadeTasks);
        }

        private async UniTask CrossFade(string key, PlayMode playMode)
        {
            crossFadeTasks.Clear();
            crossFadeTasks.Add(FadeUpTask(key, 1f, playMode));

            if (!string.IsNullOrEmpty(currentKey))
                crossFadeTasks.Add(FadeDownTask(currentKey, 0f));

            currentKey = key;

            await UniTask.WhenAll(crossFadeTasks);
        }

        private async UniTask FadeUpTask(string key, float targetVolume, PlayMode playMode)
        {
            if (!actives.ContainsKey(key))
            {
                var rented = audioSourceProvider.Rent();
                rented.loop = true;
                rented.volume = 0;
                actives.Add(key, rented);
            }

            var targetSource = actives[key];

            if (!audioBank.Audios.TryGet(key, out var clip))
            {
#if DEVELOPMENT

#endif
                return;
            }

            targetSource.clip = clip;
            int stepResolution = 100;
            float duration = 1f;
            float playbackTime = actives.ContainsKey(currentKey) && playMode == PlayMode.Transit ? actives[currentKey].time : 0;
            var volumeStep = (targetVolume - targetSource.volume) / stepResolution;
            var waitTime = duration / stepResolution;
            targetSource.time = playbackTime;

            if (!targetSource.isPlaying)
                targetSource.Play();

            while (targetSource.volume < targetVolume)
            {
                targetSource.volume += volumeStep;
                await UniTask.Delay(Mathf.CeilToInt(1000 * waitTime), DelayType.DeltaTime);
            }

            targetSource.volume = targetVolume;
        }

        private async UniTask FadeDownTask(string key, float targetVolume)
        {
            if (!actives.ContainsKey(key))
                return;

            var targetSource = actives[key];
            int stepResolution = 100;
            float duration = 1f;
            var volumeStep = (targetSource.volume - targetVolume) / stepResolution;
            var waitTime = duration / stepResolution;

            while (targetSource.volume > targetVolume)
            {
                targetSource.volume -= volumeStep;
                await UniTask.Delay(Mathf.CeilToInt(1000 * waitTime) , DelayType.DeltaTime);
            }

            targetSource.volume = targetVolume;
            targetSource.Stop();
            actives.Remove(key);
            audioSourceProvider.Return(targetSource);
        }
    }
}