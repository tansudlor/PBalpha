using System;
namespace com.playbux.effects
{
    [Serializable]
    public class EffectPotency
    {
        public EffectPotencyType type;
        public float potency;

        public EffectPotency(EffectPotencyType type, float potency)
        {
            this.type = type;
            this.potency = potency;
        }
    }
}