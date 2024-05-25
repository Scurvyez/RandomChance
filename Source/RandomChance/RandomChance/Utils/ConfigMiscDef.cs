using System.Collections.Generic;
using Verse;

namespace RandomChance
{
    public class ConfigMiscDef : Def
    {
        public float randomProductSkillsFactor = 1f;
        public FloatRange cookingBonusRangeOne = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeTwo = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeThree = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeFour = FloatRange.ZeroToOne;
        public IntRange additionalMeatStackCount = IntRange.zero;
        public IntRange repairFailureExplosionRadius = IntRange.zero;
        public IntRange repairFailureExpolsionDamageAmount = IntRange.zero;
        public int repairFailureStunDuration = 1;
        public IntRange repairFailureFireSize = IntRange.zero;
        public int farmAnimalInjuryStunDuration = 1;
        public IntRange plantWorkEggDiscoveryCount = IntRange.zero;
        public float miningExtraProductSkillsFactor = 1f;
        
        public FloatRange shockFleckRChannel = FloatRange.Zero;
        public FloatRange shockFleckGChannel = FloatRange.Zero;
        public FloatRange shockFleckBChannel = FloatRange.Zero;
    }
}
