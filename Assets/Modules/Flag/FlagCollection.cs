using com.playbux.api;
using com.playbux.events;
using com.playbux.identity;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace com.playbux.flag
{

    public interface IFlagCollection<T>
    {
        public Task<string> SetFlag(T id, string flag, string value, string questID, bool isFirstTime = false);
        public string GetFlag(T id, string flag);
        public Task Remove(T id, string flag);
        public bool HasFlag(T id);
        public string Report(T id);
        public void  CreateFlags(T id);
        public void Clear(T id);

        public void MyCallbackMethod(string message);

    }
    public partial class FlagCollectionBase<T> : IFlagCollection<T>
    {
        private Dictionary<T, Dictionary<string, string>> flag;
        private IIdentitySystem identitySystem;
        private SignalBus signalBus;

        public FlagCollectionBase(IIdentitySystem identitySystem, SignalBus signalBus)
        {
            this.identitySystem = identitySystem;
            this.signalBus = signalBus;
            flag = new Dictionary<T, Dictionary<string, string>>();
        }

        public string GetFlag(T id, string flag)
        {

            if (!this.flag.ContainsKey(id))
            {
                return null;
            }

            if (!this.flag[id].ContainsKey(flag))
            {
                return null;
            }

            return this.flag[id][flag];

        }

        public async Task<string> SetFlag(T id, string flag, string value, string questID, bool isFirstTime = false)
        {
            if (!this.flag.ContainsKey(id))
            {
                this.flag[id] = new Dictionary<string, string>();
            }

            this.flag[id][flag] = value;

            if (isFirstTime == true)
            {
                return value;
            }

            
#if SERVER

            await AddFlagToServer(id, flag, questID);
#endif
#if !SERVER
            await Task.Delay(1);
#endif
            return value;
        }

        public async Task Remove(T id, string flag)
        {
            if (!this.flag.ContainsKey(id))
            {
                return;
            }

            if (!this.flag[id].ContainsKey(flag))
            {
                return;
            }

            this.flag[id].Remove(flag);

            var stringUID = id.ToString();

            uint netID = identitySystem.NameReverse[stringUID];

            await APIServerConnector.RemoveUserFlag(id.ToString(), flag, identitySystem[netID].AccessToken);
        }

        public void Clear(T id)
        {
            if (!this.flag.ContainsKey(id))
            {
                return;
            }

            this.flag.Remove(id);
        }

        public string Report(T id)
        {
            string report = "";
            List<FlagUpdate> allFlags = new List<FlagUpdate>();
            if (!flag.ContainsKey(id))
            {
                return "ID Not Found";
            }

            foreach (var key in flag[id].Keys)
            {
                report += key + ":" + flag[id][key] + "\n";
                FlagUpdate flags = new FlagUpdate();
                flags.uid = id.ToString();
                flags.flag = key;
                flags.value = flag[id][key];
                allFlags.Add(flags);
            }

            return JsonConvert.SerializeObject(allFlags);
        }

        public bool HasFlag(T id)
        {
            if (!flag.ContainsKey(id))
            {
                return false;
            }

            if (!(flag[id].Keys.Count > 0))
            {
                return false;
            }

            return true;
        }

        public void MyCallbackMethod(string message)
        {
            throw new System.NotImplementedException();
        }

        public async Task AddFlagToServer(T id, string flag, string questID)
        {

            var type = APIServerConnector.flagRewardCostData[flag].Type;
            var stringUID = id.ToString();
            uint netID = identitySystem.NameReverse[stringUID];

            Debug.Log("AddFlagToServer : " + type + " : " + stringUID + " : "  + netID);

            if (type == "NORMAL")
            {
                await APIServerConnector.AddUserFlag(stringUID, flag, identitySystem[netID].AccessToken);
            }
            else if (type == "QUEST")
            {
                string balance = await APIServerConnector.AddQuestFlag(stringUID, flag, questID);

                if (balance == "updatebalance")
                {

                    UserProfile userProfile = await APIServerConnector.GetMe(identitySystem[netID].AccessToken);

                    if (userProfile != null)
                    {
                        Wallet wallet = userProfile.wallet;
                        int brk = wallet.brk.amount_unsafe;
                        int lotto = wallet.lotto_ticket.amount_unsafe;
                        int pebble = wallet.pebble.amount_unsafe;
                        int pbux = wallet.pbux.amount_unsafe;
                        int lottoPiece = wallet.lotto_piece.amount_unsafe;

                        //FIXME: : add This after API return


                        UserBalance userBalance = new UserBalance();
                        userBalance.Brk = brk;
                        userBalance.Lotto = lotto;
                        userBalance.Pebble = pebble;
                        userBalance.Pbux = pbux;
                        userBalance.LottoPiece = lottoPiece;


                        QuestRewardSignal questRewardSignal = new QuestRewardSignal();
                        questRewardSignal.Command = "questrewardupdate";
                        questRewardSignal.NetId = netID;
                        questRewardSignal.Data = userBalance;
                        signalBus.Fire(questRewardSignal);
                    }
                    else
                    {
                        return;
                    }

                }
                else
                {
                    return;
                }
            }

        }

        public void CreateFlags(T id)
        {
            if (!this.flag.ContainsKey(id))
            {
                this.flag[id] = new Dictionary<string, string>();
            }
        }
    }
}
