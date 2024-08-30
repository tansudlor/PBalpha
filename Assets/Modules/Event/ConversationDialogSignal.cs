using UnityEngine;

namespace com.playbux.events
{
    public struct ConversationDialogSignal
    {

    }

    public struct AuthenticationSignal
    {

    }

    public struct QuestRewardSignal
    {
        public string Command;
        public uint NetId;
        public object Data;
    }

    public struct IdentityChangeSignal
    {
        public string Command;
        public object Data;
    }

    public struct ChangeThisPlayerNameSignal
    {
       	public string ThisNameChange;
        public uint NetId;
    }

    public struct TeleportationInitiateSignal
    {
        public uint netId;
        public Vector2 keyPosition;
        public Vector2 projectedPosition;

        public TeleportationInitiateSignal(uint netId, Vector2 keyPosition, Vector2 projectedPosition)
        {
            this.netId = netId;
            this.keyPosition = keyPosition;
            this.projectedPosition = projectedPosition;
        }
    }

    public struct TeleportationCompleteSignal
    {
        public uint netId;
        public Vector2 teleportedPosition;

        public TeleportationCompleteSignal(uint netId, Vector2 teleportedPosition)
        {
            this.netId = netId;
            this.teleportedPosition = teleportedPosition;
        }
    }
    public struct LogoffSignal
    {

    }

    public struct LoginSignal
    {

    }

    public struct RefreshIconSignal
    {

    }


    public struct QuizTimeEventSignal
    {
        public string Command;
        public string Message;
        public object Data;
    }

    public struct SettingDataSignal
    {
        public string Command;
        public object Data;
    }

    public struct RemoteConfigFetchRequestSignal
    {
        public readonly string key;
        public readonly int type;

        public RemoteConfigFetchRequestSignal(string key, int type)
        {
            this.key = key;
            this.type = type;
        }
    }

    public struct RemoteConfigResponseSignal<T>
    {
        public readonly string key;
        public readonly T value;

        public RemoteConfigResponseSignal(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class CTETopFiveDataSignal
    {
        public int[] ranks = new int[5];
        public int[] totalPixels = new int[5];
        public string[] colors = new string[5];
        public string[] countryNames = new string[5];
    }

    public struct BGMPlaySignal
    {
        public string key;
        public SETHD.Echo.PlayMode playMode;
        
        public BGMPlaySignal(string key, SETHD.Echo.PlayMode playMode)
        {
            this.key = key;
            this.playMode = playMode;
        }
    }
    
    public struct BGMStopSignal
    {
        public string key;

        public BGMStopSignal(string key)
        {
            this.key = key;
        }
    }
    
    public struct BGMStopAllSignal
    {

    }

    public struct SFXPlaySignal
    {
        public string key;

        public SFXPlaySignal(string key)
        {
            this.key = key;
        }
    }

    public struct LinkOutSignal
    {
        public object linkOutData;

        public LinkOutSignal(object linkOutData)
        {
            this.linkOutData = linkOutData;
        }
    }

    public struct NotificationUISignal
    {
        public int type;
        public string desc;

        public NotificationUISignal(int type, string desc)
        {
            this.type = type;
            this.desc = desc;
        }
    }

    public struct OnStartClientSignal
    {
        
    }

    public struct SetBotEnableSignal
    {
        public bool isEnabled;

        public SetBotEnableSignal(bool isEnabled)
        {
            this.isEnabled = isEnabled;
        }
    }

    public struct SetEnemyTargetSignal
    {
        public uint NetId => netId;

        public string Name => name;

        public int MaxHp => maxHp;

        public int CurrentHp => currentHp;

        private readonly uint netId;
        private readonly string name;
        private readonly int maxHp;
        private readonly int currentHp;
        
        public SetEnemyTargetSignal(uint netId, string name, int maxHp, int currentHp)
        {
            this.netId = netId;
            this.name = name;
            this.maxHp = maxHp;
            this.currentHp = currentHp;
        }
    }
}
