using System;
using Mirror;
using UnityEngine;
using Newtonsoft.Json;
using com.playbux.enemy;

namespace com.playbux.networking.mirror.message
{
    //For WhiteListOnly
    public readonly struct AuthenticationMessage : IEquatable<AuthenticationMessage>, NetworkMessage
    {
        public readonly string Token;

        public AuthenticationMessage(string token)
        {
            Token = token;
        }

        public bool Equals(AuthenticationMessage other)
        {
            return Token == other.Token;
        }

        public override bool Equals(object obj)
        {
            return obj is AuthenticationMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Token != null ? Token.GetHashCode() : 0);
        }
    }

    public class EquipmentsRef
    {
        public string hat { get; set; }
        public string head { get; set; }
        public string face { get; set; }
        public string shirt { get; set; }
        public string pants { get; set; }
        public string back { get; set; }
        public string shoes { get; set; }
    }

    public struct Equipments
    {
        public string hat { get; set; }
        public string head { get; set; }
        public string face { get; set; }
        public string shirt { get; set; }
        public string pants { get; set; }
        public string back { get; set; }
        public string shoes { get; set; }
        public string JSONForAvatarSet()
        {
            // API  FORMAT--->  COLLECTION_ID   //API
            // GAME FORMAT--->  ID_COLLECTION   //AvartarSet
            EquipmentsRef export = new EquipmentsRef();
            var property = GetType().GetProperties();
            for (int i = 0; i < property.Length; i++)
            {
                string equip = (string)GetType().GetProperty(property[i].Name).GetValue(this);
                if (string.IsNullOrEmpty(equip))//default equiped
                {
                    equip = "default";
                }
                if (equip.IndexOf('_') < 0)// DEFAULT , 51 , 1
                {
                    equip = "1_" + equip; //to Normal API FORM -> COLLECTION_ID ,   1_default ,1_51 , 1_1
                }

                string[] factor = equip.Split('_');
                string reform = factor[1] + "_" + factor[0];  // to GAME FORM -> ID_COLLECTION , default_1 , 51_1 , 1_1
                export.GetType().GetProperty(property[i].Name).SetValue(export, reform);

            }
            return JsonConvert.SerializeObject(export);
        }
    }

    public readonly struct StartCastMessage : IEquatable<StartCastMessage>, NetworkMessage
    {
        public readonly uint castId;
        public readonly uint casterId;
        public readonly uint targetId;
        public readonly uint abilityId;

        /// <summary>
        ///   <para></para>
        /// </summary>
        /// <param name="castId">Instance id of this casting ability</param>
        /// <param name="casterId">Mirror's net id of network identity who cast this ability</param>
        /// <param name="targetId">Mirror's net id of network identity whom this cast targeted</param>
        /// <param name="abilityId">Ability id of this cast</param>
        public StartCastMessage(uint castId, uint casterId, uint targetId, uint abilityId)
        {
            this.castId = castId;
            this.casterId = casterId;
            this.targetId = targetId;
            this.abilityId = abilityId;
        }
        public bool Equals(StartCastMessage other)
        {
            return castId == other.castId && casterId == other.casterId && abilityId == other.abilityId;
        }
        public override bool Equals(object obj)
        {
            return obj is StartCastMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(castId, casterId, abilityId);
        }
    }

    public readonly struct UpdateCastMessage : IEquatable<UpdateCastMessage>, NetworkMessage
    {
        public readonly uint castId;
        public readonly uint targetId;
        public readonly uint casterId;
        public readonly uint abilityId;
        public readonly float currentCastTime;

        /// <summary>
        ///   <para></para>
        /// </summary>
        /// <param name="currentCastTime">current time of cast on the server</param>
        /// <param name="castId">Instance id of this casting ability</param>
        /// <param name="targetId">Mirror's net id of network identity whom this cast targeted</param>;
        /// <param name="casterId">Mirror's net id of network identity who cast this ability</param>
        /// <param name="abilityId">Ability id of this cast</param>
        public UpdateCastMessage(float currentCastTime, uint castId, uint targetId, uint casterId, uint abilityId)
        {
            this.castId = castId;
            this.targetId = targetId;
            this.casterId = casterId;
            this.abilityId = abilityId;
            this.currentCastTime = currentCastTime;
        }
        public bool Equals(UpdateCastMessage other)
        {
            return currentCastTime == other.currentCastTime && castId == other.castId && casterId == other.casterId && abilityId == other.abilityId;
        }
        public override bool Equals(object obj)
        {
            return obj is UpdateCastMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(currentCastTime, castId, casterId, abilityId);
        }
    }

    public readonly struct CancelCastMessage : IEquatable<CancelCastMessage>, NetworkMessage
    {
        public readonly uint castId;
        public readonly uint abilityId;

        /// <summary>
        ///   <para>Check if a collider overlaps a point in space.</para>
        /// </summary>
        /// <param name="castId">Instance id of this casting ability</param>
        /// <param name="abilityId">Ability id of this cast</param>
        public CancelCastMessage(uint castId, uint abilityId)
        {
            this.castId = castId;
            this.abilityId = abilityId;
        }
        public bool Equals(CancelCastMessage other)
        {
            return castId == other.castId && abilityId == other.abilityId;
        }
        public override bool Equals(object obj)
        {
            return obj is CancelCastMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(castId, abilityId);
        }
    }

    public readonly struct EndCastMessage : IEquatable<EndCastMessage>, NetworkMessage
    {
        public readonly uint castId;
        public readonly uint casterId;
        public readonly uint abilityId;

        public EndCastMessage(uint castId, uint casterId, uint abilityId)
        {
            this.castId = castId;
            this.casterId = casterId;
            this.abilityId = abilityId;
        }
        public bool Equals(EndCastMessage other)
        {
            return castId == other.castId && abilityId == other.abilityId;
        }
        public override bool Equals(object obj)
        {
            return obj is EndCastMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(castId, abilityId);
        }
    }

    public readonly struct EnemyMoveMessage : IEquatable<EnemyMoveMessage>, NetworkMessage
    {
        public readonly uint id;
        public readonly Vector2 targetPosition;

        public EnemyMoveMessage(uint id, Vector2 targetPosition)
        {
            this.id = id;
            this.targetPosition = targetPosition;
        }
        public bool Equals(EnemyMoveMessage other)
        {
            return id == other.id && targetPosition.Equals(other.targetPosition);
        }
        public override bool Equals(object obj)
        {
            return obj is EnemyMoveMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id, targetPosition);
        }
    }

    public readonly struct EnemyAttackMessage : IEquatable<EnemyAttackMessage>, NetworkMessage
    {
        public readonly uint id;
        public readonly uint targetId;

        public EnemyAttackMessage(uint id, uint targetId)
        {
            this.id = id;
            this.targetId = targetId;
        }
        public bool Equals(EnemyAttackMessage other)
        {
            return id == other.id && targetId == other.targetId;
        }
        public override bool Equals(object obj)
        {
            return obj is EnemyAttackMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id, targetId);
        }
    }

    public readonly struct EnemyTurnMessage : IEquatable<EnemyTurnMessage>, NetworkMessage
    {
        public readonly uint id;
        public readonly Vector2 direction;

        public EnemyTurnMessage(uint id, Vector2 direction)
        {
            this.id = id;
            this.direction = direction;
        }
        public bool Equals(EnemyTurnMessage other)
        {
            return id == other.id && direction.Equals(other.direction);
        }
        public override bool Equals(object obj)
        {
            return obj is EnemyTurnMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id, direction);
        }
    }

    public readonly struct EnemyStateChangeMessage : IEquatable<EnemyStateChangeMessage>, NetworkMessage
    {
        public readonly uint id;
        public readonly EnemyState state;
        public EnemyStateChangeMessage(uint id, EnemyState state)
        {
            this.id = id;
            this.state = state;
        }
        public bool Equals(EnemyStateChangeMessage other)
        {
            return id == other.id && state == other.state;
        }
        public override bool Equals(object obj)
        {
            return obj is EnemyStateChangeMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(id, (int)state);
        }
    }

    public readonly struct FATEStartMessage : IEquatable<FATEStartMessage>, NetworkMessage
    {
        public readonly uint fateId;

        public FATEStartMessage(uint fateId)
        {
            this.fateId = fateId;
        }

        public bool Equals(FATEStartMessage other)
        {
            return fateId == other.fateId;
        }

        public override bool Equals(object obj)
        {
            return obj is FATEStartMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)fateId;
        }
    }

    public readonly struct FATENotificationMessage : IEquatable<FATENotificationMessage>, NetworkMessage
    {
        public readonly DateTimeByte time;
        public readonly uint fateId;
        public readonly string message;

        public FATENotificationMessage(DateTimeByte time, uint fateId, string message)
        {
            this.time = time;
            this.fateId = fateId;
            this.message = message;
        }

        public bool Equals(FATENotificationMessage other)
        {
            return time.Equals(other.time) && fateId == other.fateId;
        }
        public override bool Equals(object obj)
        {
            return obj is FATENotificationMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(time, fateId, message);
        }
    }

    public struct FATEDataPack : IEquatable<FATEDataPack>
    {
        public uint[] ids;

        public FATEDataPack(uint[] ids)
        {
            this.ids = ids;
        }

        public bool Equals(FATEDataPack other)
        {
            return Equals(ids, other.ids);
        }

        public override bool Equals(object obj)
        {
            return obj is FATEDataPack other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (ids != null ? ids.GetHashCode() : 0);
        }
    }

    public readonly struct FATEListUpdateMessage : IEquatable<FATEListUpdateMessage>, NetworkMessage
    {
        public readonly DayOfWeek dayOfWeek;
        public readonly DateTimeByte[] keys;
        public readonly FATEDataPack[] fateIds;

        public FATEListUpdateMessage(DayOfWeek dayOfWeek, DateTimeByte[] keys, FATEDataPack[] fateIds)
        {
            this.dayOfWeek = dayOfWeek;
            this.keys = keys;
            this.fateIds = fateIds;
        }

        public bool Equals(FATEListUpdateMessage other)
        {
            return dayOfWeek == other.dayOfWeek && Equals(keys, other.keys) && Equals(fateIds, other.fateIds);
        }

        public override bool Equals(object obj)
        {
            return obj is FATEListUpdateMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)dayOfWeek, keys, fateIds);
        }
    }

    public readonly struct CreateCharacterMessage : IEquatable<CreateCharacterMessage>, NetworkMessage
    {
        public readonly string Name;

        public CreateCharacterMessage(string name)
        {
            Name = name;
        }

        public bool Equals(CreateCharacterMessage other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is CreateCharacterMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public readonly struct PlayerListMessage : IEquatable<PlayerListMessage>, NetworkMessage
    {
        public readonly string[] Names;
        public readonly uint[] NetIds;

        public PlayerListMessage(string[] names, uint[] netIds)
        {
            Names = names;
            NetIds = netIds;
        }

        public bool Equals(PlayerListMessage other)
        {
            return Equals(Names, other.Names) && Equals(NetIds, other.NetIds);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerListMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Names, NetIds);
        }
    }

    public readonly struct ChatBroadcastMessage : IEquatable<ChatBroadcastMessage>, NetworkMessage
    {
        public readonly long timestamp;
        public readonly ushort chatLevel;
        public readonly string sender;
        public readonly string message;

        public ChatBroadcastMessage(long timestamp, ushort chatLevel, string sender, string message)
        {
            this.timestamp = timestamp;
            this.chatLevel = chatLevel;
            this.sender = sender;
            this.message = message;
        }

        public bool Equals(ChatBroadcastMessage other)
        {
            return timestamp == other.timestamp && chatLevel == other.chatLevel && sender == other.sender && message == other.message;
        }
        public override bool Equals(object obj)
        {
            return obj is ChatBroadcastMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(timestamp, chatLevel, sender, message);
        }
    }

    public readonly struct ChatCommandMessage : IEquatable<ChatCommandMessage>, NetworkMessage
    {
        public readonly long timestamp;
        public readonly string sender;
        public readonly string message;

        public ChatCommandMessage(long timestamp, string sender, string message)
        {
            this.timestamp = timestamp;
            this.sender = sender;
            this.message = message;
        }
        public bool Equals(ChatCommandMessage other)
        {
            return timestamp == other.timestamp && sender == other.sender && message == other.message;
        }
        public override bool Equals(object obj)
        {
            return obj is ChatCommandMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(timestamp, sender, message);
        }
    }

    public readonly struct PlayerMoveInputMessage : IEquatable<PlayerMoveInputMessage>, NetworkMessage
    {
        public readonly double[] Timestamps;
        public readonly Vector2[] Inputs;

        public PlayerMoveInputMessage(double[] timestamps, Vector2[] inputs)
        {
            Inputs = inputs;
            Timestamps = timestamps;
        }


        public bool Equals(PlayerMoveInputMessage other)
        {
            return Timestamps.Equals(other.Timestamps) && Inputs.Equals(other.Inputs);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerMoveInputMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Timestamps, Inputs);
        }
    }

    public readonly struct PlayerUpdatePositionMessage : IEquatable<PlayerUpdatePositionMessage>, NetworkMessage
    {
        public readonly UInt16 NetId;
        public readonly double[] Timestamps;
        public readonly Vector2[] Inputs;
        public readonly Vector3[] Position;

        public PlayerUpdatePositionMessage(UInt16 netId, double[] timestamps, Vector2[] inputs, Vector3[] position)
        {
            NetId = netId;
            Timestamps = timestamps;
            Inputs = inputs;
            Position = position;
        }

        public bool Equals(PlayerUpdatePositionMessage other)
        {
            return NetId == other.NetId && Equals(Timestamps, other.Timestamps) && Equals(Inputs, other.Inputs) && Equals(Position, other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerUpdatePositionMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Timestamps, Inputs, Position);
        }
    }

    public readonly struct OtherPlayerUpdatePositionMessage : IEquatable<OtherPlayerUpdatePositionMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly double[] Timestamps;
        public readonly Vector2[] Inputs;
        public readonly Vector3[] Position;

        public OtherPlayerUpdatePositionMessage(uint netId, double[] timestamps, Vector2[] inputs, Vector3[] position)
        {
            NetId = netId;
            Timestamps = timestamps;
            Inputs = inputs;
            Position = position;
        }

        public bool Equals(OtherPlayerUpdatePositionMessage other)
        {
            return NetId == other.NetId && Equals(Timestamps, other.Timestamps) && Equals(Inputs, other.Inputs) && Equals(Position, other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is OtherPlayerUpdatePositionMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Timestamps, Inputs, Position);
        }
    }

    public readonly struct MapDataMessage : IEquatable<MapDataMessage>, NetworkMessage
    {
        public readonly string Name;
        public MapDataMessage(string name)
        {
            Name = name;
        }
        public bool Equals(MapDataMessage other)
        {
            return Name == other.Name;
        }
        public override bool Equals(object obj)
        {
            return obj is MapDataMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public readonly struct MapColliderUpdateMessage : IEquatable<MapColliderUpdateMessage>, NetworkMessage
    {
        public readonly string Name;

        public MapColliderUpdateMessage(string name)
        {
            Name = name;
        }
        public bool Equals(MapColliderUpdateMessage other)
        {
            return Name == other.Name;
        }
        public override bool Equals(object obj)
        {
            return obj is MapColliderUpdateMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }

    public readonly struct PropColliderUpdateMessage : IEquatable<PropColliderUpdateMessage>, NetworkMessage
    {
        public readonly string[] Names;
        public readonly int[] Positions;
        public readonly Vector3[] Scales;
        public readonly Quaternion[] Rotations;

        public PropColliderUpdateMessage(string[] names, int[] positions, Quaternion[] rotations, Vector3[] scales)
        {
            Names = names;
            Scales = scales;
            Positions = positions;
            Rotations = rotations;
        }

        public bool Equals(PropColliderUpdateMessage other)
        {
            return Equals(Names, other.Names) && Equals(Scales, other.Scales) && Equals(Rotations, other.Rotations) && Equals(Positions, other.Positions);
        }

        public override bool Equals(object obj)
        {
            return obj is PropColliderUpdateMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Names, Scales, Rotations, Positions);
        }
    }

    public readonly struct TeleportationRequestMessage : IEquatable<TeleportationRequestMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly Vector2 KeyPosition;
        public readonly Vector2 ProjectedPosition;

        public TeleportationRequestMessage(uint netId, Vector2 keyPosition, Vector2 projectedPosition)
        {
            NetId = netId;
            KeyPosition = keyPosition;
            ProjectedPosition = projectedPosition;
        }
        public bool Equals(TeleportationRequestMessage other)
        {
            return NetId == other.NetId && KeyPosition.Equals(other.KeyPosition) && ProjectedPosition.Equals(other.ProjectedPosition);
        }
        public override bool Equals(object obj)
        {
            return obj is TeleportationRequestMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, KeyPosition, ProjectedPosition);
        }
    }

    public readonly struct TeleportationInvalidMessage : IEquatable<TeleportationInvalidMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public TeleportationInvalidMessage(uint netId)
        {
            NetId = netId;
        }

        public bool Equals(TeleportationValidMessage other)
        {
            return NetId == other.NetId;
        }
        public override bool Equals(object obj)
        {
            return obj is TeleportationInvalidMessage other && Equals(other);
        }
        public bool Equals(TeleportationInvalidMessage other)
        {
            return NetId == other.NetId;
        }
    }

    public readonly struct TeleportationValidMessage : IEquatable<TeleportationValidMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly bool IsInside;
        public readonly Vector2 TargetPosition;

        public TeleportationValidMessage(uint netId, bool isInside, Vector2 targetPosition)
        {
            NetId = netId;
            IsInside = isInside;
            TargetPosition = targetPosition;
        }
        public bool Equals(TeleportationValidMessage other)
        {
            return NetId == other.NetId && TargetPosition.Equals(other.TargetPosition) && IsInside == other.IsInside;
        }
        public override bool Equals(object obj)
        {
            return obj is TeleportationValidMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, IsInside, TargetPosition);
        }
    }

    public readonly struct EntityDespawnMessage : IEquatable<EntityDespawnMessage>, NetworkMessage
    {
        public readonly uint NetId;

        public EntityDespawnMessage(uint netId)
        {
            NetId = netId;
        }
        public bool Equals(EntityDespawnMessage other)
        {
            return NetId == other.NetId;
        }
        public override bool Equals(object obj)
        {
            return obj is EntityDespawnMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return (int)NetId;
        }
    }

    public readonly struct EntityEffectUpdateMessage : IEquatable<EntityEffectUpdateMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly uint[] EffectIds;
        public readonly float[] EffectDurations;

        public EntityEffectUpdateMessage(uint netId, uint[] effectIds, float[] effectDurations)
        {
            NetId = netId;
            EffectIds = effectIds;
            EffectDurations = effectDurations;
        }
        public bool Equals(EntityEffectUpdateMessage other)
        {
            return NetId == other.NetId && Equals(EffectIds, other.EffectIds) && Equals(EffectDurations, other.EffectDurations);
        }
        public override bool Equals(object obj)
        {
            return obj is EntityEffectUpdateMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, EffectIds, EffectDurations);
        }
    }

    public readonly struct EnemySpawnMessage : IEquatable<EnemySpawnMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly uint EnemyId;

        public EnemySpawnMessage(uint netId, uint enemyId)
        {
            NetId = netId;
            EnemyId = enemyId;
        }

        public bool Equals(EnemySpawnMessage other)
        {
            return NetId == other.NetId && EnemyId == other.EnemyId;
        }

        public override bool Equals(object obj)
        {
            return obj is EnemySpawnMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, EnemyId);
        }
    }

    public readonly struct MoveCommandMessage : IEquatable<MoveCommandMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly ushort Frame;
        public readonly uint CommandId;
        public readonly byte[] CommandDatas;
        public readonly int CommandDataSize;

        public MoveCommandMessage(uint netId, ushort frame, uint commandId, byte[] commandDatas, int commandDataSize)
        {
            NetId = netId;
            Frame = frame;
            CommandId = commandId;
            CommandDatas = commandDatas;
            CommandDataSize = commandDataSize;
        }

        public override bool Equals(object obj)
        {
            return obj is MoveCommandMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Frame, CommandId, CommandDatas, CommandDataSize);
        }

        public bool Equals(MoveCommandMessage other)
        {
            return NetId == other.NetId && Frame == other.Frame && CommandId == other.CommandId && Equals(CommandDatas, other.CommandDatas) && CommandDataSize == other.CommandDataSize;
        }
    }

    public readonly struct MoveCommandValidationMessage : IEquatable<MoveCommandValidationMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly ushort Frame;
        public readonly uint CommandId;
        public readonly Vector2 Position;

        public MoveCommandValidationMessage(uint netId, ushort frame, uint commandId, Vector2 position)
        {
            NetId = netId;
            Frame = frame;
            CommandId = commandId;
            Position = position;
        }
        public bool Equals(MoveCommandValidationMessage other)
        {
            return NetId == other.NetId && Frame == other.Frame && CommandId == other.CommandId && Position.Equals(other.Position);
        }
        public override bool Equals(object obj)
        {
            return obj is MoveCommandValidationMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Frame, CommandId, Position);
        }
    }

    public readonly struct FakePlayerPartMessage : IEquatable<FakePlayerPartMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly string[] PartId;

        public FakePlayerPartMessage(uint netId, string[] partId)
        {
            NetId = netId;
            PartId = partId;
        }
        public bool Equals(FakePlayerPartMessage other)
        {
            return NetId == other.NetId && PartId == other.PartId;
        }
        public override bool Equals(object obj)
        {
            return obj is FakePlayerPartMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, PartId);
        }
    }

    public readonly struct FakePlayerPositionMessage : IEquatable<FakePlayerPositionMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly double[] Timestamps;
        public readonly Vector2[] Inputs;
        public readonly Vector3[] Position;

        public FakePlayerPositionMessage(uint netId, double[] timestamps, Vector2[] inputs, Vector3[] position)
        {
            NetId = netId;
            Timestamps = timestamps;
            Inputs = inputs;
            Position = position;
        }

        public bool Equals(FakePlayerPositionMessage other)
        {
            return NetId == other.NetId && Equals(Timestamps, other.Timestamps) && Equals(Inputs, other.Inputs) && Equals(Position, other.Position);
        }

        public override bool Equals(object obj)
        {
            return obj is FakePlayerPositionMessage other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Timestamps, Inputs, Position);
        }
    }

    public readonly struct FakePlayerNameChangeMessage : IEquatable<FakePlayerNameChangeMessage>, NetworkMessage
    {
        public readonly uint NetId;
        public readonly string Name;
        public FakePlayerNameChangeMessage(uint netId, string name)
        {
            NetId = netId;
            Name = name;
        }
        public bool Equals(FakePlayerNameChangeMessage other)
        {
            return NetId == other.NetId && Name == other.Name;
        }
        public override bool Equals(object obj)
        {
            return obj is FakePlayerNameChangeMessage other && Equals(other);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(NetId, Name);
        }
    }
}