using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace RandomChance
{
    [UsedImplicitly]
    public class ConfigMiscDef : Def
    {
        public FloatRange shockFleckRChannel = FloatRange.ZeroToOne;
        public FloatRange shockFleckGChannel = FloatRange.ZeroToOne;
        public FloatRange shockFleckBChannel = FloatRange.ZeroToOne;
        
        public FloatRange cookingBonusRangeOne = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeTwo = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeThree = FloatRange.ZeroToOne;
        public FloatRange cookingBonusRangeFour = FloatRange.ZeroToOne;
        public IntRange additionalMeatStackCount = IntRange.zero;
        public IntRange repairFailureExplosionRadius = IntRange.zero;
        public IntRange repairFailureExplosionDamageAmount = IntRange.zero;
        public IntRange repairFailureFireSize = IntRange.zero;
        public IntRange plantWorkEggDiscoveryCount = IntRange.zero;
        public IntRange buildingRemovalSilverFindCount = IntRange.zero;

        public float buildingRemovalSilverFindChance = 1f;
        
        public float randomProductSkillsFactor = 1f;
        public int repairFailureStunDuration = 1;
        public int farmAnimalInjuryStunDuration = 1;
        public float miningExtraProductSkillsFactor = 1f;
    }
}