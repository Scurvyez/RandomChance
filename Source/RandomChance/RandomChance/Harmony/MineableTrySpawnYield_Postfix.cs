using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;
using System;

namespace RandomChance
{
    /*
    [HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
    public class MineableTrySpawnYield_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref Map map, bool moteOnWaste, Pawn pawn, Mineable __instance)
        {
            FieldInfo yieldPctField = AccessTools.Field(typeof(Mineable), "yieldPct");
            float yieldPct = (float)yieldPctField.GetValue(__instance);

            // Spawn the original mineableThing
            if (__instance.def.building.mineableThing != null && !(Rand.Value > __instance.def.building.mineableDropChance))
            {
                int num = Mathf.Max(1, __instance.def.building.EffectiveMineableYield);
                if (__instance.def.building.mineableYieldWasteable)
                {
                    num = Mathf.Max(1, GenMath.RoundRandom((float)num * yieldPct));
                }
                Thing thing2 = ThingMaker.MakeThing(__instance.def.building.mineableThing);
                thing2.stackCount = num;
                GenPlace.TryPlaceThing(thing2, __instance.Position, map, ThingPlaceMode.Near);
            }

            if (!pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
            {
                float skillsFactor = 0.20f;
                RandomProductExtension rpEx = __instance.def.GetModExtension<RandomProductExtension>();
                int pawnsAvgSkillLevel = (int)pawn.skills.AverageOfRelevantSkillsFor(pawn.CurJob.workGiverDef.workType);
                SimpleCurve extraYieldCurve = RandomChance_DefOf.RC_Curves.extraMiningYieldCurve;

                if (rpEx != null && rpEx.randomProducts != null && rpEx.randomProductChance.HasValue && rpEx.randomProductChance.Value > 0f
                        && Rand.Chance(rpEx.randomProductChance.Value))
                {
                    if (Rand.Chance(extraYieldCurve.Evaluate(pawnsAvgSkillLevel)))
                    {
                        float totalWeight = 0f;
                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            totalWeight += productData.randomProductWeight;
                        }

                        float randomValue = Rand.Range(0f, totalWeight);
                        float accumulatedWeight = 0f;

                        ThingDef selectedDef = null;

                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            accumulatedWeight += productData.randomProductWeight;

                            if (randomValue <= accumulatedWeight)
                            {
                                selectedDef = DefDatabase<ThingDef>.GetNamed(productData.randomProduct.defName, errorOnFail: false);

                                if (selectedDef != null)
                                {
                                    Thing thingToSpawn = ThingMaker.MakeThing(selectedDef);

                                    FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                                    int skillBasedSpawnCount = Mathf.RoundToInt(pawnsAvgSkillLevel * skillsFactor);

                                    FloatRange newMinSpawnCountRange;
                                    FloatRange newMaxSpawnCountRange;

                                    newMinSpawnCountRange = new FloatRange(initialSpawnCountRange.min, initialSpawnCountRange.min * skillBasedSpawnCount);
                                    newMaxSpawnCountRange = new FloatRange(initialSpawnCountRange.max, initialSpawnCountRange.max * skillBasedSpawnCount);

                                    int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, (int)newMinSpawnCountRange.max);
                                    int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, (int)newMaxSpawnCountRange.max);

                                    thingToSpawn.stackCount = Rand.RangeInclusive(newMinSpawnCount, newMaxSpawnCount);
                                    GenPlace.TryPlaceThing(thingToSpawn, __instance.Position, map, ThingPlaceMode.Near);
                                }

                                break; // Exit the loop after selecting one item.
                            }
                        }
                    }
                }
            }
        }
    }
    */
}
