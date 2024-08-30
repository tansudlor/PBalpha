using System;
using com.playbux.networking.mirror.core;
using UnityEngine;
namespace com.playbux.networking.mirror.client.chat
{
    [Serializable]
    public class ChatTabSettings
    {
        public bool ableToRemove;
        public string name;
        public Sprite icon;
        public ChatLevel[] filter;
    }
}