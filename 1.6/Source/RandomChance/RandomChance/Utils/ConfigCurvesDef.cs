using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace RandomChance
{
    [UsedImplicitly]
    public class ConfigCurvesDef : Def
    {
        public SimpleCurve powerArmorInjuryCurve;
        public SimpleCurve betterMealOutcomeCurve;
        public SimpleCurve brokenBuildingDamageCurve;
        public SimpleCurve hurtByFarmAnimalCurve;
        public SimpleCurve plantWorkDiscoveryCurve;
        public SimpleCurve agitatedWildAnimalCurve;
        public SimpleCurve extraMiningYieldCurve;
        public SimpleCurve cookingFailureCurve;
        public SimpleCurve butcheringMessCurve;
        public SimpleCurve crematingInjuryCurve;
        public SimpleCurve butcherBonusProductsCurve;
        public SimpleCurve switchFlickingSpreeCurve;
        
        private const string ConfigErrorSuffix = " is null, ensure the curve and it's points are defined in xml!";
        
        public override IEnumerable<string> ConfigErrors()
        {
            if (powerArmorInjuryCurve == null)
            {
                yield return $"{RCLog.ModName}powerArmorInjuryCurve {ConfigErrorSuffix}";
            }
            else if (betterMealOutcomeCurve == null)
            {
                yield return $"{RCLog.ModName}betterMealOutcomeCurve {ConfigErrorSuffix}";
            }
            else if (brokenBuildingDamageCurve == null)
            {
                yield return $"{RCLog.ModName}brokenBuildingDamageCurve {ConfigErrorSuffix}";
            }
            else if (hurtByFarmAnimalCurve == null)
            {
                yield return $"{RCLog.ModName}hurtByFarmAnimalCurve {ConfigErrorSuffix}";
            }
            else if (plantWorkDiscoveryCurve == null)
            {
                yield return $"{RCLog.ModName}plantWorkDiscoveryCurve {ConfigErrorSuffix}";
            }
            else if (agitatedWildAnimalCurve == null)
            {
                yield return $"{RCLog.ModName}agitatedWildAnimalCurve {ConfigErrorSuffix}";
            }
            else if (extraMiningYieldCurve == null)
            {
                yield return $"{RCLog.ModName}extraMiningYieldCurve {ConfigErrorSuffix}";
            }
            else if (cookingFailureCurve == null)
            {
                yield return $"{RCLog.ModName}cookingFailureCurve {ConfigErrorSuffix}";
            }
            else if (butcheringMessCurve == null)
            {
                yield return $"{RCLog.ModName}butcheringMessCurve {ConfigErrorSuffix}";
            }
            else if (crematingInjuryCurve == null)
            {
                yield return $"{RCLog.ModName}crematingInjuryCurve {ConfigErrorSuffix}";
            }
            else if (butcherBonusProductsCurve == null)
            {
                yield return $"{RCLog.ModName}butcherBonusProductsCurve {ConfigErrorSuffix}";
            }
            else if (switchFlickingSpreeCurve == null)
            {
                yield return $"{RCLog.ModName}switchFlickingSpreeCurve {ConfigErrorSuffix}";
            }
        }
    }
}