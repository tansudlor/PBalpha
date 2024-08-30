using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.playbux.ability
{
    [Serializable]
    public class AbilityData
    {
        public string name;
        public string desc;
        public float castTime;
        public float animationTime = 1f;
        public AbilityType abilityType;
        public RecastData recastTime;
        public uint[] effects;
        public AbilityPotency[] potencies;
    }

    [Serializable]
    public enum AbilityType
    {
        Weaponskill,
        Circuitcast,
        Ability
    }

    [Serializable]
    public class RecastData
    {
        [FormerlySerializedAs("recastTime")]
        public float time;
        public string recastStack;
    }

    [Serializable]
    public class AbilityPotency
    {
        public AbilityPotencyType type;
        public int potency;

        public AbilityPotency(AbilityPotencyType type, int potency)
        {
            this.type = type;
            this.potency = potency;
        }
    }

    [Serializable]
    public class AbilityAssetData
    {
        public GameObject asset;
    }

    [Serializable]
    public enum AbilityPotencyType
    {
        Flat = 0,
        Percentage,
        Fixed,
    }
}