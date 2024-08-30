using System;
namespace com.playbux.effects
{
    [Serializable]
    public class PermanentEffectData
    {
        public string name;
        public string desc;
        public string stackId;
        public EffectPotency[] potencies;

        public PermanentEffectData Clone()
        {
            var cloneData = new PermanentEffectData();
            cloneData.name = name;
            cloneData.desc = desc;
            cloneData.potencies = potencies;
            return cloneData;
        }
    }
}