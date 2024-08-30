using UnityEngine;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace SETHD.Echo
{
    public class SfxChannel : IAudioChannel
    {
        private bool isPaused;
        
        private readonly AudioBank audioBank;
        private readonly Dictionary<string, AudioSource> actives;
        private readonly AudioSourceProvider audioSourceProvider;
        
        public SfxChannel(AudioBank audioBank, AudioSourceProvider audioSourceProvider)
        {
            this.audioBank = audioBank;
            this.audioSourceProvider = audioSourceProvider;
            actives = new Dictionary<string, AudioSource>();
        }

        public void Reinitialize()
        {

        }

        public async UniTask Play(string key, PlayMode playMode = PlayMode.StartOver)
        {
            if (!audioBank.Audios.TryGet(key, out var value))
                return;
            
            var rented = audioSourceProvider.Rent();
            if (!actives.ContainsKey(key))
            {
                actives.Add(key, rented);
            }
            Assert.IsTrue(value);
            rented.clip = value;
            rented.Play();
            await UniTask.Yield();
        }

        public async UniTask Pause()
        {
            foreach (var source in actives)
            {
                if (isPaused)
                {
                    source.Value.UnPause();
                    return;
                }
                
                source.Value.Pause();
            }

            isPaused = !isPaused;
            await UniTask.Yield();
        }

        public async UniTask Stop()
        {
            foreach (var source in actives)
            {
                source.Value.Stop();
                audioSourceProvider.Return(source.Value);
            }
            
            await UniTask.Yield();
        }

        public async UniTask Stop(string key)
        {
            if (!actives.ContainsKey(key))
                return;
            
            actives[key].UnPause();
            audioSourceProvider.Return(actives[key]);
            actives.Remove(key);
            await UniTask.Yield();
        }
    }
}