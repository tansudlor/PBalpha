using System;
using UnityEngine;

namespace com.playbux.utilis.network
{
    public enum RemoteConfigType
    {
        String = 0,
        Int,
        Float,
        Boolean
    }
    
    public interface IRemoteConfigKeyPair<T>
    {
        T Value { get; set; }
        string Key { get; }
        RemoteConfigType Type { get; }
    }
    
    [Serializable]
    public abstract class RemoteConfig<T> : IRemoteConfigKeyPair<T>
    {
        public T Value
        {
            get => value; 
            set => this.value = value; 
        }
        public string Key => key;
        public abstract RemoteConfigType Type { get; }

        [SerializeField]
        private string key;

        [SerializeField]
        private T value;
    }

    [Serializable]
    public class RemoteConfigString : RemoteConfig<string>
    {
        public override RemoteConfigType Type => RemoteConfigType.String;
    }
    
    [Serializable]
    public class RemoteConfigInt : RemoteConfig<int>
    {
        public override RemoteConfigType Type => RemoteConfigType.Int;
    }
}