using System;
using System.Collections.Generic;

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GenRecipeMakeRecipeProducts_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, Thing dominantIngredient, IBillGiver billGiver)
        {
            if (!worker.IsColonyMech)
            {
                RandomProductExtension rpEx = recipeDef.GetModExtension<RandomProductExtension>();
                int pawnsAvgSkillLevel = (int)worker.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
                float skillsFactor = 0.20f;

                List<Thing> modifiedProducts = new();

                foreach (Thing product in __result)
                {
                    if (rpEx != null && rpEx.randomProducts != null && rpEx.randomProductChance.HasValue && rpEx.randomProductChance.Value > 0f && Rand.Value < rpEx.randomProductChance.Value)
                    {
                        float totalWeight = 0f;
                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            totalWeight += productData.randomProductWeight;
                        }

                        float randomValue = Rand.Range(0f, totalWeight);
                        float accumulatedWeight = 0f;

                        foreach (RandomProductData productData in rpEx.randomProducts)
                        {
                            accumulatedWeight += productData.randomProductWeight;

                            if (randomValue <= accumulatedWeight)
                            {
                                ThingDef selectedDef = DefDatabase<ThingDef>.GetNamed(productData.randomProduct.defName, errorOnFail: false);
                                if (selectedDef != null)
                                {
                                    if (recipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        for (int i = 0; i < rpEx.randomProducts.Count; i++)
                                        {
                                            if (product.def == rpEx.randomProducts[i].randomProduct)
                                            {
                                                product.def = rpEx.randomProducts[i].randomProduct;

                                                SimpleCurve bonusSpawnCurve = new()
                                            {
                                                { 0, 0 },
                                                { 5, 0 },
                                                { 8, Rand.RangeInclusive(0, 1) },
                                                { 14, Rand.RangeInclusive(0, 2) },
                                                { 18, Rand.RangeInclusive(0, 3) },
                                                { 20, Rand.RangeInclusive(0, 4) }
                                            };

                                                int bonusSpawnCount = (int)bonusSpawnCurve.Evaluate(pawnsAvgSkillLevel);
                                                int finalSpawnCount = product.stackCount + bonusSpawnCount;
                                                product.stackCount = finalSpawnCount;

                                                if (bonusSpawnCount > 0)
                                                {
                                                    Messages.Message("RC_BonusMealProduct".Translate(worker.Named("PAWN"), finalSpawnCount, Find.ActiveLanguageWorker.Pluralize(product.def.label, finalSpawnCount)), worker, MessageTypeDefOf.PositiveEvent);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        product.def = selectedDef;
                                        FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                                        int skillBasedSpawnCount = Mathf.RoundToInt(pawnsAvgSkillLevel * skillsFactor);

                                        FloatRange newMinSpawnCountRange;
                                        FloatRange newMaxSpawnCountRange;

                                        newMinSpawnCountRange = new FloatRange(initialSpawnCountRange.min, initialSpawnCountRange.min * skillBasedSpawnCount);
                                        newMaxSpawnCountRange = new FloatRange(initialSpawnCountRange.max, initialSpawnCountRange.max * skillBasedSpawnCount);

                                        int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, (int)newMinSpawnCountRange.max);
                                        int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, (int)newMaxSpawnCountRange.max);

                                        product.stackCount = Rand.RangeInclusive(newMinSpawnCount, newMaxSpawnCount);
                                        Messages.Message("RC_RandomStoneCuttingProduct".Translate(worker.Named("PAWN"), product.stackCount, Find.ActiveLanguageWorker.Pluralize(product.def.label, product.stackCount), dominantIngredient.def.label), worker, MessageTypeDefOf.PositiveEvent);
                                    }
                                }
                                break;
                            }
                        }
                    }
                    modifiedProducts.Add(product);
                }
                __result = modifiedProducts;
            }
        }
    }
}
