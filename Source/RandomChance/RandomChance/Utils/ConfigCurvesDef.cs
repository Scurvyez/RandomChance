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
        
        public override IEnumerable<string> ConfigErrors()
        {
            if (powerArmorInjuryCurve == null)
            {
                yield return "[RandomChance] powerArmorInjuryCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (betterMealOutcomeCurve == null)
            {
                yield return "[RandomChance] betterMealOutcomeCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (brokenBuildingDamageCurve == null)
            {
                yield return "[RandomChance] brokenBuildingDamageCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (hurtByFarmAnimalCurve == null)
            {
                yield return "[RandomChance] hurtByFarmAnimalCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (plantWorkDiscoveryCurve == null)
            {
                yield return "[RandomChance] plantWorkDiscoveryCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (agitatedWildAnimalCurve == null)
            {
                yield return "[RandomChance] agitatedWildAnimalCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (extraMiningYieldCurve == null)
            {
                yield return "[RandomChance] extraMiningYieldCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (cookingFailureCurve == null)
            {
                yield return "[RandomChance] cookingFailureCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (butcheringMessCurve == null)
            {
                yield return "[RandomChance] butcheringMessCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (crematingInjuryCurve == null)
            {
                yield return "[RandomChance] crematingInjuryCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (butcherBonusProductsCurve == null)
            {
                yield return "[RandomChance] butcherBonusProductsCurve is null, ensure the curve and it's points are defined in xml!";
            }
            else if (switchFlickingSpreeCurve == null)
            {
                yield return "[RandomChance] switchFlickingSpreeCurve is null, ensure the curve and it's points are defined in xml!";
            }
        }
    }
}