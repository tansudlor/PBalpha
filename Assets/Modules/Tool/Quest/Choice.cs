using Newtonsoft.Json;
using UnityEngine;

namespace com.playbux.tool
{
    [System.Serializable]
    public class Choice
    {
        [SerializeField]
        private string message;
        [SerializeField]
        private string next;

        [JsonProperty("message")]
        public string Message { get => message; set => message = value; }
        [JsonProperty("next")]
        public string Next { get => next; set => next = value; }
    }

}
