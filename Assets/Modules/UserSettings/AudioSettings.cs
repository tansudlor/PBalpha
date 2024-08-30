using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace com.playbux.settings
{
    [Serializable]
    public class AudioSettings
    {
        [SerializeField]
        private bool mute;
        [SerializeField]
        private SerializedDictionary<string, float> audioLevels = new SerializedDictionary<string, float>();

        public bool Mute { get => mute; set => mute = value; }
        public SerializedDictionary<string, float> AudioLevels { get => audioLevels; set => audioLevels = value; }
    }
}