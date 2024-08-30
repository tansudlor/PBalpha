using System;
using UnityEngine.Serialization;
namespace com.playbux.effects
{
    [Serializable]
    public class TemporaryEffectData
    {
        public string name;
        public string desc;
        public float duration;
        [FormerlySerializedAs("stackName")]
        public string stackId;
        public EffectPotency[] potencies;

        public TemporaryEffectData Clone()
        {
            var cloneData = new TemporaryEffectData();
            cloneData.name = name;
            cloneData.desc = desc;
            cloneData.duration = duration;
            cloneData.stackId = stackId;
            cloneData.potencies = potencies;
            return cloneData;
        }
    }
}