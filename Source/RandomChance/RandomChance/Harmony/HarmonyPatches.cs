using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse.AI;
using RandomChance.MapComps;
using System.Reflection;
using System.Reflection.Emit;

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.scurvyez.randomchance");

            harmony.Patch(original: AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeRecipeProductsPrefix)));

            harmony.Patch(original: AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeRecipeProductsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_FixBrokenDownBuilding), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(FixBrokenDownBuildingMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_GatherAnimalBodyResources), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GatherAnimalBodyResourcesMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_PlantWork), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PlantWorkMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_Wear), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(WearMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TrySpawnYieldPostFix)));

            harmony.Patch(AccessTools.Method(typeof(Toils_Recipe), nameof(Toils_Recipe.DoRecipeWork)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoRecipeWorkTranspiler)));
        }
        
        private static readonly Dictionary<RecipeDef, RecipeDef> recipeMap = new()
        {
            { RandomChance_DefOf.CookMealSimple, RandomChance_DefOf.CookMealFine },
            { RandomChance_DefOf.CookMealFine, RandomChance_DefOf.CookMealLavish },
            { RandomChance_DefOf.CookMealSimpleBulk, RandomChance_DefOf.CookMealFineBulk },
            { RandomChance_DefOf.CookMealFine_Veg, RandomChance_DefOf.CookMealLavish_Veg },
            { RandomChance_DefOf.CookMealFine_Meat, RandomChance_DefOf.CookMealLavish_Meat },
            { RandomChance_DefOf.CookMealFineBulk, RandomChance_DefOf.CookMealLavishBulk },
            { RandomChance_DefOf.CookMealFineBulk_Veg, RandomChance_DefOf.CookMealLavishBulk_Veg },
            { RandomChance_DefOf.CookMealFineBulk_Meat, RandomChance_DefOf.CookMealLavishBulk_Meat }
        };
        
        public static void MakeRecipeProductsPrefix(ref RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
        {
            if (!worker.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
            {
                int pawnsAvgSkillLevel = (int)worker.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
                float betterQualityMealChance = RandomChanceSettings.CookingBetterMealChance; // 5%
                SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.betterMealOutcomeCurve;

                if (Rand.Chance(betterQualityMealChance))
                {
                    if (recipeMap.TryGetValue(recipeDef, out RecipeDef newRecipeDef))
                    {
                        if (Rand.Chance(injuryCurve.Evaluate(pawnsAvgSkillLevel)))
                        {
                            recipeDef = newRecipeDef;

                            if (RandomChanceSettings.AllowMessages)
                            {
                                Messages.Message("RC_BetterMealProduced".Translate(worker.Named("PAWN")),
                                MessageTypeDefOf.PositiveEvent);
                            }
                        }
                    }
                }
            }
        }

        public static void MakeRecipeProductsPostfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, Thing dominantIngredient)
        {
            if (recipeDef == null) return;
            RandomProductExtension rpEx = recipeDef.GetModExtension<RandomProductExtension>();

            if (rpEx == null) return;
            {
                if (!worker.IsColonyMech)
                {
                    float pawnsAvgSkillLevel = worker.skills.AverageOfRelevantSkillsFor(worker.CurJob.workGiverDef.workType);
                    float skillsFactor = 0.20f;

                    List<Thing> modifiedProducts = new();

                    foreach (Thing product in __result)
                    {
                        if (rpEx != null && rpEx.randomProducts != null && rpEx.randomProductChance.HasValue && rpEx.randomProductChance.Value > 0f
                            && Rand.Chance(rpEx.randomProductChance.Value))
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

                                                    if (bonusSpawnCount > 0 && RandomChanceSettings.AllowMessages)
                                                    {
                                                        Messages.Message("RC_BonusMealProduct".Translate(worker.Named("PAWN"),
                                                            product.Label), worker, MessageTypeDefOf.PositiveEvent);
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

                                            if (RandomChanceSettings.AllowMessages)
                                            {
                                                Messages.Message("RC_RandomStoneCuttingProduct".Translate(worker.Named("PAWN"),
                                                dominantIngredient.Label, product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        if (recipeDef == RandomChance_DefOf.ButcherCorpseFlesh && Rand.Chance(RandomChanceSettings.BonusButcherProductChance))
                        {
                            Thing butcheredCorpse = worker.CurJob.GetTarget(TargetIndex.B).Thing;

                            SimpleCurve chanceCurve = new() // add a new xml curve for this one!
                            {
                                { 0, 0.025f },
                                { 3, 0.05f },
                                { 6, 0.09f },
                                { 8, 0.2f },
                                { 14, 0.3f },
                                { 18, 0.4f },
                                { 20, 0.5f }
                            };

                            if (butcheredCorpse is Corpse corpse)
                            {
                                if (corpse.InnerPawn.RaceProps.predator)
                                {
                                    if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                                    {
                                        int additionalMeatStackCount = Rand.RangeInclusive(1, 15);
                                        float butcheredCorpseBodySizeFactor = (corpse.InnerPawn.RaceProps.baseBodySize / 1.15f);
                                        butcheredCorpseBodySizeFactor = Mathf.Max(butcheredCorpseBodySizeFactor, 1f);

                                        ThingDef meatDef = GetRandomMeatFromOtherPawn();
                                        Thing additionalMeat = ThingMaker.MakeThing(meatDef);
                                        additionalMeat.stackCount = additionalMeatStackCount * Mathf.RoundToInt(butcheredCorpseBodySizeFactor);

                                        if (RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_PredatorButcheryBonusProduct".Translate(worker.Named("PAWN"),
                                            additionalMeat.Label, dominantIngredient.Label), worker, MessageTypeDefOf.PositiveEvent);
                                        }

                                        modifiedProducts.Add(additionalMeat);
                                    }
                                }
                            }
                        }
                        modifiedProducts.Add(product);
                    }
                    __result = modifiedProducts;
                }
            }
        }

        private static ThingDef GetRandomMeatFromOtherPawn()
        {
            List<ThingDef> meatDefs = DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(def => def.IsWithinCategory(ThingCategoryDefOf.MeatRaw))
                    .ToList();

            if (meatDefs.Count > 0)
                return meatDefs.RandomElement();
            else
                return ThingDefOf.Meat_Human;
        }
        
        public static void FixBrokenDownBuildingMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        float averageSkills = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                        SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.brokenBuildingDamageCurve;
                        Building building = __instance.job.GetTarget(TargetIndex.A).Thing as Building;

                        if (Rand.Chance(RandomChanceSettings.ElectricalRepairFailureChance) && Rand.Chance(injuryCurve.Evaluate(averageSkills)))
                        {
                            if (building != null)
                            {
                                IntVec3 explosionCenter = building.Position;
                                Map explosionMap = building.Map;
                                int explosionRadius = Rand.RangeInclusive(1, 5);

                                GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.EMP, null, Rand.RangeInclusive(1, 9));
                                if (Rand.Value < 0.25f)
                                {
                                    GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.Bomb, null, Rand.RangeInclusive(3, 12));
                                    FireUtility.TryStartFireIn(building.Position, building.Map, RandomChanceSettings.FailedCookingFireSize, null); // CHANGE?
                                    MoteMaker.MakeColonistActionOverlay(__instance.pawn, ThingDefOf.Mote_ColonistFleeing);
                                }

                                if (RandomChanceSettings.AllowMessages)
                                {
                                    Messages.Message("RC_ElectricalRepairFailure".Translate(__instance.pawn.Named("PAWN")),
                                    __instance.pawn, MessageTypeDefOf.NegativeEvent);
                                }

                                if (Rand.Value < 0.20f)
                                {
                                    IncidentDef incident = IncidentDef.Named("ShortCircuit");
                                    IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, building.Map);
                                    incident.Worker.TryExecute(parms);
                                }
                            }

                            BodyPartRecord bodyPart = __instance.pawn.RaceProps.body.corePart;

                            if (bodyPart != null)
                            {
                                HediffDef electricShock = RandomChance_DefOf.RC_ElectricShockHediff;

                                Hediff hediff = HediffMaker.MakeHediff(electricShock, __instance.pawn);
                                hediff.Severity = Rand.Value;
                                __instance.pawn.health.AddHediff(hediff, bodyPart);

                                /*Effecter shockEffect;

                                shockEffect = RandomChance_DefOf.RC_ElectricShockBonesEffect.Spawn();
                                shockEffect.Trigger(__instance.pawn, __instance.pawn);

                                shockEffect.Cleanup();
                                shockEffect = null;*/

                                Find.TickManager.slower.SignalForceNormalSpeedShort();
                                __instance.pawn.stances.stunner.StunFor(60, __instance.pawn, false, false);
                                __instance.EndJobWith(JobCondition.Incompletable);
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
        
        public static void GatherAnimalBodyResourcesMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_GatherAnimalBodyResources __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.hurtByFarmAnimalCurve;
                        float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                        if (Rand.Chance(RandomChanceSettings.HurtByFarmAnimalChance) && Rand.Chance(injuryCurve.Evaluate(pawnsAvgSkillLevel)))
                        {
                            if (__instance.job.GetTarget(TargetIndex.A).Thing is not Pawn animal) return;

                            if (animal != null)
                            {
                                List<Tool> tools = animal.Tools;

                                if (tools != null && tools.Count > 0)
                                {
                                    Tool selectedTool = tools.RandomElement();

                                    if (Rand.Chance(selectedTool.chanceFactor) && selectedTool != null)
                                    {
                                        float damageAmount = Rand.Range(selectedTool.power / 2f, selectedTool.power);
                                        DamageDef damageInflicted = selectedTool.Maneuvers.RandomElement().verb.meleeDamageDef;
                                        DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f);
                                        __instance.pawn.TakeDamage(damageInfo);

                                        if (damageAmount > 0f && RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_HurtByFarmAnimal".Translate(__instance.pawn.Named("PAWN"),
                                                animal.NameShortColored), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                                        }

                                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                                        __instance.pawn.stances.stunner.StunFor(60, __instance.pawn, false, false);
                                        __instance.EndJobWith(JobCondition.Incompletable);
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
        
        public static void PlantWorkMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_PlantWork __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    Map map = __instance.job.GetTarget(TargetIndex.A).Thing.Map;

                    if (RandomChance_DefOf.RC_Curves != null && map != null && !__instance.pawn.IsColonyMech)
                    {
                        MapComponent_CollectThings thingCollections = map.GetComponent<MapComponent_CollectThings>();

                        SimpleCurve discoveryCurve = RandomChance_DefOf.RC_Curves.plantWorkDiscoveryCurve;
                        float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                        if (thingCollections != null)
                        {
                            ThingDef chosenEggDef = thingCollections.possibleEggs.RandomElement();

                            if (chosenEggDef != null && chosenEggDef.GetCompProperties<CompProperties_Hatcher>() != null)
                            {
                                if (Rand.Chance(RandomChanceSettings.PlantHarvestingFindEggsChance) && Rand.Chance(discoveryCurve.Evaluate(pawnsAvgSkillLevel)))
                                {
                                    Thing eggs = ThingMaker.MakeThing(chosenEggDef, null);
                                    eggs.stackCount = Rand.RangeInclusive(1, 3);
                                    GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);

                                    if (RandomChanceSettings.AllowMessages)
                                    {
                                        Messages.Message("RC_PlantWorkFoundEggs".Translate(__instance.pawn.Named("PAWN"),
                                            eggs.Label), __instance.pawn, MessageTypeDefOf.PositiveEvent);
                                    }
                                }

                                Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;
                                SimpleCurve agitatedAnimalCurve = RandomChance_DefOf.RC_Curves.agitatedWildAnimalCurve;
                                if (Rand.Chance(agitatedAnimalCurve.Evaluate(pawnsAvgSkillLevel)))
                                {
                                    PawnKindDef agitatedAnimalKind = chosenEggDef.GetCompProperties<CompProperties_Hatcher>().hatcherPawn;

                                    if (!targetThing.def.plant.sowTags.Contains("Hydroponic") || !targetThing.Position.Roofed(map))
                                    {
                                        IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(__instance.pawn.Position, map, 1);
                                        Pawn agitatedAnimal = PawnGenerator.GeneratePawn(agitatedAnimalKind, null);
                                        agitatedAnimal.gender = Gender.Female;
                                        GenSpawn.Spawn(agitatedAnimal, spawnCell, map);
                                        agitatedAnimal.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Manhunter);

                                        if (RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_PlantWorkAngryMomma".Translate(__instance.pawn.Named("PAWN"),
                                                agitatedAnimal.Label), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }

        public static void WearMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_Wear __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        DamageDef damageInflicted = DamageDefOf.Cut;
                        SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.powerArmorInjuryCurve;
                        float pawnsSkillLevel = __instance.pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
                        float qualityFactor = QualityGetter.GetQualityValue(__instance.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompQuality>().Quality, injuryCurve);
                        List<BodyPartRecord> digits = __instance.pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit);
                        ThingDef apparelPiece = __instance.job.GetTarget(TargetIndex.A).Thing.def;

                        if (Rand.Chance(RandomChanceSettings.InjuredByApparelChance) && Rand.Chance(injuryCurve.Evaluate(pawnsSkillLevel)))
                        {
                            float damageAmount = Rand.Range(0.1f, pawnsSkillLevel) * qualityFactor;

                            if (apparelPiece == RandomChance_DefOf.Apparel_PowerArmor) // body
                            {
                                DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, digits.RandomElement());
                                __instance.pawn.TakeDamage(damageInfo);
                            }
                            else if (apparelPiece == RandomChance_DefOf.Apparel_PowerArmorHelmet) // head
                            {
                                DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, __instance.pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head).RandomElement());
                                __instance.pawn.TakeDamage(damageInfo);
                            }

                            if (RandomChanceSettings.AllowMessages)
                            {
                                Messages.Message("RC_InjuryWhileDonningArmor".Translate(__instance.pawn.Named("PAWN"),
                                    apparelPiece.label), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
        
        public static void TrySpawnYieldPostFix(ref Map map, bool moteOnWaste, Pawn pawn, Mineable __instance)
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

        public static IEnumerable<CodeInstruction> DoRecipeWorkTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo target = AccessTools.Method(typeof(IBillGiverWithTickAction), "UsedThisTick");
            MethodBase addition = AccessTools.Method(typeof(HarmonyPatches), nameof(TryGiveRandomFailure));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(target))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // Load 'actor' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // Load 'curJob' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // Load 'jobDriver' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // Load 'building' onto the stack
                    yield return new CodeInstruction(OpCodes.Call, addition); // Call the TryGiveRandomFailure method
                    continue;
                }
            }
        }

        public static void TryGiveRandomFailure(Pawn actor, Job curJob, JobDriver_DoBill jobDriver, Building_WorkTable building)
        {
            Log.Message("TryGiveRandomFailure Running");
            if (!actor.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
            {
                bool startFire = false;
                bool giveInjury = false;

                SimpleCurve cookingFailureCurve = RandomChance_DefOf.RC_Curves.cookingFailureCurve;
                SimpleCurve butcheringMessCurve = RandomChance_DefOf.RC_Curves.butcheringMessCurve;
                SimpleCurve crematingInjuryCurve = RandomChance_DefOf.RC_Curves.crematingInjuryCurve;
                int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);
                building = curJob.GetTarget(TargetIndex.A).Thing as Building_WorkTable;

                if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(RandomChanceSettings.CookingFailureChance))
                        {
                            if (Rand.Chance(cookingFailureCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                if (Rand.Bool)
                                {
                                    startFire = true;
                                }
                                else
                                {
                                    giveInjury = true;
                                }
                            }
                        }
                    }

                    if (startFire)
                    {
                        StartFireHandler(actor, curJob, building);
                    }
                    if (giveInjury)
                    {
                        GiveInjuryHandler(actor, curJob, building);
                    }
                }

                if (curJob.RecipeDef == RandomChance_DefOf.ButcherCorpseFlesh)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(RandomChanceSettings.ButcheringFailureChance))
                        {
                            if (Rand.Chance(butcheringMessCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                CauseMessHandler(actor, curJob, building);
                            }
                        }
                    }
                }

                if (curJob.RecipeDef == RandomChance_DefOf.CremateCorpse)
                {
                    if (jobDriver.ticksSpentDoingRecipeWork == 1)
                    {
                        if (Rand.Chance(RandomChanceSettings.CrematingInjuryChance))
                        {
                            if (Rand.Chance(crematingInjuryCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                GiveInjuryHandler(actor, curJob, building);
                            }
                        }
                    }
                }
            }
        }

        private static void StartFireHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (RandomChanceSettings.AllowMessages)
            {
                Messages.Message("RC_FireInKitchen".Translate(actor.Named("PAWN")), actor, MessageTypeDefOf.NegativeEvent);
            }

            Thing ingredients = curJob.GetTarget(TargetIndex.B).Thing;
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;

            if (ingredients != null)
            {
                if (!ingredients.Destroyed)
                {
                    ingredients.Destroy();
                }

                FireUtility.TryStartFireIn(buildingPos, map, RandomChanceSettings.FailedCookingFireSize, null);
                MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistFleeing);
                Find.TickManager.slower.SignalForceNormalSpeedShort();
                actor.stances.stunner.StunFor(120, actor, false, false);
            }
        }

        private static void GiveInjuryHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;
            HediffDef burnHediffDef = RandomChance_DefOf.Burn;
            HediffDef cutHediffDef = HediffDefOf.Cut;

            float severity = pawnsAvgSkillLevel switch
            {
                < 5 => 0.4f,
                <= 10 => 0.2f,
                <= 15 => 0.08f,
                <= 18 => 0.02f,
                <= 20 => 0f,
                _ => 0f,
            };

            if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BodyPartRecord fingersPart = actor.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit).RandomElement();

                if (fingersPart != null)
                {
                    if (Rand.Bool)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, fingersPart);
                        hediff.Severity = severity;
                        actor.health.AddHediff(hediff);

                        if (RandomChanceSettings.AllowMessages)
                        {
                            Messages.Message("RC_InjuryInKitchen".Translate(actor.Named("PAWN")),
                                actor, MessageTypeDefOf.NegativeEvent);
                        }
                    }
                    else
                    {
                        Hediff hediff = HediffMaker.MakeHediff(cutHediffDef, actor, fingersPart);
                        hediff.Severity = severity;
                        actor.health.AddHediff(hediff);

                        if (RandomChanceSettings.AllowMessages)
                        {
                            Messages.Message("RC_InjuryInKitchen".Translate(actor.Named("PAWN")),
                                actor, MessageTypeDefOf.NegativeEvent);
                        }

                        IntVec3 adjacentCell = buildingPos + GenAdj.CardinalDirections.RandomElement();
                        FilthMaker.TryMakeFilth(adjacentCell, map, ThingDefOf.Filth_Blood);
                    }
                }
            }
            else if (curJob.RecipeDef == RandomChance_DefOf.CremateCorpse)
            {
                BodyPartRecord bodyPart = actor.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbSegment).RandomElement();

                if (bodyPart != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, bodyPart);
                    hediff.Severity = severity;
                    actor.health.AddHediff(hediff);

                    if (RandomChanceSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryWhileCremating".Translate(actor.Named("PAWN")),
                            actor, MessageTypeDefOf.NegativeEvent);
                    }
                }
            }
        }

        private static void CauseMessHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (curJob.GetTarget(TargetIndex.B).Thing is Corpse animalCorpse)
            {
                IntVec3 pawnPos = actor.Position;
                Map map = building.Map;
                int radius = RandomChanceSettings.ButcherMessRadius; // make a mod setting
                IntVec3 centerCell = pawnPos + GenAdj.CardinalDirections.RandomElement();
                Pawn animalPawn = animalCorpse.InnerPawn;

                Region region = centerCell.GetRegion(map);
                if (region != null)
                {
                    foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, radius, true))
                    {
                        if (cell.GetRegion(map) == region)
                        {
                            FilthMaker.TryMakeFilth(cell, map, animalPawn.def.race.BloodDef);
                        }
                    }
                }
                else
                {
                    FilthMaker.TryMakeFilth(centerCell, map, animalPawn.def.race.BloodDef);
                }

                if (RandomChanceSettings.AllowMessages)
                {
                    Messages.Message("RC_HorriblyUncleanKitchen".Translate(actor.Named("PAWN")),
                        actor, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
    }
}
