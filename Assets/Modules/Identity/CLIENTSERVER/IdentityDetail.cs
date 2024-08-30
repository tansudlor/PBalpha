using Mirror;
using UnityEngine;
using Newtonsoft.Json;
using com.playbux.networking.mirror.message;
using System;
using com.playbux.api;
using System.Collections.Generic;





#if SERVER
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
#endif

namespace com.playbux.identity
{
    public class IdentityDetail
    {
        private static Dictionary<string, string> variableCoverter = new Dictionary<string, string>()
                {{"PEBBLE","BalancePebble"},{"BRK","BalanceBrk" },{ "PBUX","BalancePbux"},{"LOTTO_TICKET","BalanceLottoTickets" },{"LOTTO_PIECE","BalanceLottoPiece" },{"WEEKLY_POINT","BalanceWeeklyPoint" } };
        private static Dictionary<string, string> variableToLower = new Dictionary<string, string>()
                { {"PEBBLE","Pebble"},{"BRK","BRK" },{ "PBUX","PBUX"},{"LOTTO_TICKET","Lotto Tickets" },{"LOTTO_PIECE","Lotto Piece" },{"WEEKLY_POINT","Weekly Point"}};
        private IIdentityObserver observer;//client only
        private NetworkIdentity networkIdentity; //dont send to client let client get it 
        private GameObject gameObject; //dont send to client let client get it 
        private string uid;
        private string id;
        private string userName;
        private uint netId;
        private string displayName;
        private string accessToken;
        private Equipments equipments;
        private int balanceBrk;
        private int balanceLottoTickets;
        private DateTime loginTime;
        private Wallet wallet;
        private int balancePebble;
        private bool canPlayQuiz;
        private string email;


        [JsonIgnore]
        public static Dictionary<string, string> VariableCoverter { get => variableCoverter; }
        [JsonIgnore]
        public static Dictionary<string, string> VariableToLower { get => variableToLower; }
        [JsonIgnore] //only client assign this property
        public NetworkIdentity Identity { get => networkIdentity; set => networkIdentity = value; }
        [JsonIgnore] //only client assign this property
        public GameObject GameObject { get => gameObject; set => gameObject = value; }
        public string UID { get => uid; set => uid = value; }
        public string UserName { get => userName; set => userName = value; }
        public uint NetId
        {
            get
            {
                return netId;
            }
            set
            {
                netId = value;
            }
        }

        public string DisplayName { get => displayName; set => displayName = value; }
        public Equipments Equipments { get => equipments; set => equipments = value; }
        [JsonIgnore]
        public IIdentityObserver Observer { get => observer; set => observer = value; }
        public string AccessToken { get => accessToken; set => accessToken = value; }
        public string ID { get => id; set => id = value; }
        public int BalanceBrk { get => balanceBrk; set => balanceBrk = value; }
        public int BalanceLottoTickets { get => balanceLottoTickets; set => balanceLottoTickets = value; }
        public DateTime LoginTime { get => loginTime; set => loginTime = value; }
        public Wallet Wallet { get => wallet; set => wallet = value; }
        public int BalancePebble { get => balancePebble; set => balancePebble = value; }
        public bool CanPlayQuiz { get => canPlayQuiz; set => canPlayQuiz = value; }
        public string Email { get => email; set => email = value; }
    }

}
