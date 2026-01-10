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
        public IntRange additionalMeatStackCount = IntRange.Zero;
        public IntRange repairFailureExplosionRadius = IntRange.Zero;
        public IntRange repairFailureExplosionDamageAmount = IntRange.Zero;
        public IntRange repairFailureFireSize = IntRange.Zero;
        public IntRange plantWorkEggDiscoveryCount = IntRange.Zero;
        public IntRange buildingRemovalSilverFindCount = IntRange.Zero;

        public float buildingRemovalSilverFindChance = 1f;
        
        public float randomProductSkillsFactor = 1f;
        public int repairFailureStunDuration = 1;
        public int farmAnimalInjuryStunDuration = 1;
        public float miningExtraProductSkillsFactor = 1f;
    }
}