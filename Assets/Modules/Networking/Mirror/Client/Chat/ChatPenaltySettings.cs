using System;
using UnityEngine.Serialization;
namespace com.playbux.networking.mirror.client.chat
{
    [Serializable]
    public class ChatPenaltySettings
    {
        public float cooldown = 0.33f;
        public float penaltyTime = 10f;
        public float maxPenaltyTime = 60f;
        public float penaltyIncreaseStep = 5f;
    }

    [Serializable]
    public class ChatConstrainSettings
    {
        public int maximumLetterCount = 48;
        public string[] prohibitedWords;
    }
}