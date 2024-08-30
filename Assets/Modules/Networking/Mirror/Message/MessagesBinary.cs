#if BINARY_NETWORK
using Mirror;
using System;
using System.Reflection;
using UnityEngine;

namespace com.playbux.networking.mirror.message
{
    //UserProfile
    public enum UserProfileCommand : byte
    {
        ChangeName,
        Rename,
        ChageAccessToken
    }

    public readonly struct ReDataParameter
    {
        public readonly string Name;
        public readonly string AccessToken;

        public ReDataParameter(string name, string accessToken)
        {
            Name = name;
            AccessToken = accessToken;

        }
    }


    public readonly struct UserProfileMessage : NetworkMessage
    {

        public readonly uint NetId;

        public readonly ReDataParameter ReDataParameter;
        public readonly UserProfileCommand Cmd;
        public UserProfileMessage(uint netId, UserProfileCommand cmd, ReDataParameter reDataParameter)
        {
            NetId = netId;
            Cmd = cmd;
            ReDataParameter = reDataParameter;
        }

    }

    //UserData
    public enum UserDataCommand : byte
    {
        Me,
        UserData,
        UserBalance,

    }


    public readonly struct UserDataParameter
    {
        public readonly string ID;
        public readonly string UID;
        public readonly string UserName;
        public readonly uint NetId;
        public readonly string DisplayName;
        public readonly Equipments Equipments;
        public readonly int BalanceBrk;
        public readonly int BalanceLottoTickets;
        public readonly DateTime LoginTime;
        public readonly int BalancePebble;
        public readonly string Email;

        public UserDataParameter(string id, string uID, string userName, uint netId, string displayName, Equipments equipments, int balanceBrk, int balanceLottoTickets, DateTime loginTime, int balancePebble, string email)
        {
            ID = id;
            UID = uID;
            BalanceBrk = balanceBrk;
            BalanceLottoTickets = balanceLottoTickets;
            UserName = userName;
            NetId = netId;
            DisplayName = displayName;
            Equipments = equipments;
            LoginTime = loginTime;
            BalancePebble = balancePebble;
            Email = email;
        }

        public void ApplyPropertiesObject(object data)
        {

            foreach (FieldInfo item in GetType().GetFields())
            {
                if (item.GetValue(this) == null)
                {
                    continue;
                }

                data.GetType().GetProperty(item.Name).SetValue(data, item.GetValue(this));
            }
        }


    }

    public readonly struct UserDataMessage : NetworkMessage
    {
        public readonly uint NetId;

        public readonly UserDataParameter UserDataParameter;
        public readonly UserDataCommand Cmd;
        public UserDataMessage(uint netId, UserDataCommand cmd, UserDataParameter userDataParameter)
        {
            NetId = netId;
            Cmd = cmd;
            UserDataParameter = userDataParameter;
        }

    }



    //Quest is For Marketing VIDEO Only
    public enum QuestCommand : byte
    {
        Dialog,
        RequestNextDialog,


    }

    public readonly struct QuestMessage : NetworkMessage
    {

        public readonly uint NetId;
        public readonly string Message;

        public QuestMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
        }


    }

    public readonly struct AvatarUpdateMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Message;
        public AvatarUpdateMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
        }
    }

    public readonly struct InventoryUpdateMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Message;
        public readonly string DataJson;
        public readonly InventoryCommunicateData Data;
        public InventoryUpdateMessage(uint netId, string message, InventoryCommunicateData data)
        {
            NetId = netId;
            Message = message;
            Data = data;
            DataJson = "";
        }
        public InventoryUpdateMessage(uint netId, string message, String json)
        {
            NetId = netId;
            Message = message;
            Data = new InventoryCommunicateData(null, null);
            DataJson = json;
        }
        public InventoryUpdateMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
            Data = new InventoryCommunicateData(null, null);
            DataJson = "";
        }
    }

    public readonly struct MiniEventMessage : NetworkMessage
    {
        public readonly string Command;
        public readonly string Message;
        public readonly QuestionInfo QuestionInfo;

        public MiniEventMessage(string command, string message, QuestionInfo questinInfo)
        {
            Command = command;
            Message = message;
            QuestionInfo = questinInfo;

        }
    }

    public readonly struct UserStatusMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Message;

        public UserStatusMessage(uint netId, string message)
        {
            NetId = netId;
            Message = message;
        }
    }

    public readonly struct QuestionInfo
    {

        public readonly string question;

        public readonly string[] choice;


        public QuestionInfo(string question, string[] choice)
        {
            this.question = question;
            this.choice = choice;
        }

    }

    public readonly struct VersioningInfo : NetworkMessage
    {

        public readonly string version;

        public VersioningInfo(string version)
        {
            this.version = version;
        }
    }


    public readonly struct KickToWinMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Message;
        public readonly long enterTicks;
        public readonly long serverTicks;
        public readonly int value;

        public KickToWinMessage(uint netId, string message, long enterTicks, long serverTicks, int value)
        {
            NetId = netId;
            Message = message;
            this.enterTicks = enterTicks;
            this.serverTicks = serverTicks;
            this.value = value;

        }
    }

    public readonly struct ResyncMessage : NetworkMessage
    {
        public readonly uint NetId;
        public readonly ushort Frame;

        public ResyncMessage(uint netId, ushort frame)
        {
            NetId = netId;
            Frame = frame;
        }
    }
}
#endif