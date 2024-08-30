using Zenject;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace SETHD.Echo
{
    public class AudioSourceProvider
    {
        private readonly AudioMixerGroup mixer;
        private readonly AudioSourceFactory factory;
        private readonly Stack<AudioSource> disables;

        public AudioSourceProvider(AudioMixerGroup mixer, AudioSourceFactory factory)
        {
            this.mixer = mixer;
            this.factory = factory;
            disables = new Stack<AudioSource>();
        }

        public AudioSource Rent()
        {
            var audioSource = disables.Count <= 0 ? factory.Create() : disables.Pop();
            audioSource.outputAudioMixerGroup = mixer;
            return audioSource;
        }

        public void Return(AudioSource source)
        {
            disables.Push(source);
        }
    }
}