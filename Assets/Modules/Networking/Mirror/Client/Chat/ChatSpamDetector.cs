using UnityEngine;
using Zenject;
namespace com.playbux.networking.mirror.client.chat
{
    public class ChatConstrainValidator
    {
        private readonly ChatConstrainSettings settings;

        public ChatConstrainValidator(ChatConstrainSettings settings)
        {
            this.settings = settings;
        }

        public bool HasProhibitedWord(string message)
        {
            bool hasBanWord = false;

            for (int i = 0; i < settings.prohibitedWords.Length; i++)
            {
                if (message.ToLower().Contains(settings.prohibitedWords[i].ToLower()))
                {
                    hasBanWord = true;
                    break;
                }
            }

            return hasBanWord;
        }

        public string Trim(string message)
        {
            string trimmed = "";

            for (int i = 0; i < message.Length; i++)
            {
                if (i + 1 >= settings.maximumLetterCount)
                    break;

                trimmed += message[i];
            }

            trimmed = trimmed.Replace("\n", " ");
            trimmed = trimmed.Replace("\r", " ");

            return trimmed;
        }
    }

    public class ChatSpamDetector : ILateTickable
    {
        public float PenaltyTime
        {
            get
            {
                if (countTime < 0)
                    return -countTime;

                return 0;
            }
        }

        private bool hasSentRecently;
        private bool alreadyInMaxPenalty;
        private float countTime;
        private int counterStep;
        private readonly ChatPenaltySettings settings;

        public ChatSpamDetector(ChatPenaltySettings settings)
        {
            this.settings = settings;
        }


        //NOTE: Return true if the message can be submit to the server
        public bool ValidateSpam()
        {
            if (hasSentRecently)
            {
                if (alreadyInMaxPenalty)
                    return false;

                counterStep++;
                float penaltyTime = (counterStep == 1 ? settings.penaltyTime : 0) + settings.penaltyIncreaseStep * counterStep;
                countTime -= penaltyTime;

                if (countTime <= -settings.maxPenaltyTime)
                {
                    countTime = -settings.maxPenaltyTime;
                    alreadyInMaxPenalty = true;
                }

                return false;
            }

            if (!hasSentRecently)
            {
                counterStep = 0;
                return hasSentRecently = true;
            }

            if (countTime < settings.cooldown)
                return false;

            return hasSentRecently = true;
        }

        public void LateTick()
        {
            if (!hasSentRecently)
                return;

            if (countTime > settings.cooldown)
            {
                countTime = 0;
                alreadyInMaxPenalty = hasSentRecently = false;
                return;
            }

            countTime += Time.deltaTime;
        }
    }
}