using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;

namespace com.playbux.networking.client.ability
{
    [Serializable]
    public class AbilityAnimationName<T>
    {
        public SerializedDictionary<uint, SerializedDictionary<AbilityAnimationState, T>> data = new SerializedDictionary<uint, SerializedDictionary<AbilityAnimationState, T>>();
    }

    [Serializable]
    public class AbilityAnimationStateName<T>
    {
        public SerializedDictionary<AbilityAnimationState, T> data = new SerializedDictionary<AbilityAnimationState, T>();
    }
    
    [Serializable]
    public enum AbilityAnimationState
    {
        PreCast,
        Cast,
        PostCast
    }

    [Serializable]
    public class CharacterAbilityData
    {
        public uint[] data;
    }
}