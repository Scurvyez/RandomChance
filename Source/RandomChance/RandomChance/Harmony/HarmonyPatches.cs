﻿using Verse;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse.AI;
using RandomChance.MapComps;
using System.Reflection;
#pragma warning disable CS0642 // Possible mistaken empty statement

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new (id: "rimworld.scurvyez.randomchance");
            
            harmony.Patch(original: AccessTools.Method(typeof(Toils_Recipe), "DoRecipeWork"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RecipeDoRecipeWorkPostfix)));
            
            harmony.Patch(original: AccessTools.Method(typeof(GenRecipe), nameof(GenRecipe.MakeRecipeProducts)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeRecipeProductsPrefix)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MakeRecipeProductsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_FixBrokenDownBuilding), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(FixBrokenDownBuildingMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_GatherAnimalBodyResources), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(GatherAnimalBodyResourcesMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_PlantWork), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(PlantWorkMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_Wear), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(WearMakeNewToilsPostfix)));

            harmony.Patch(original: AccessTools.Method(typeof(Mineable), "TrySpawnYield", 
                    new Type[] { typeof(Map), typeof(bool), typeof(Pawn) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TrySpawnYieldPostFix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_ViewArt), "WaitTickAction"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ViewArtWaitTickActionPrefix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_RemoveBuilding), "MakeNewToils"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RemoveBuildingMakeNewToilsPostFix)));
        }

        public static void RecipeDoRecipeWorkPostfix(Toil __result)
        {
            __result.AddPreTickAction(() =>
            {
                Pawn worker = __result.actor;
                
                if (worker?.jobs?.curJob == null) return;
                Job curJob = worker.jobs.curJob;
                int tickModulo = curJob.RecipeDef.workAmount.SecondsToTicks() / 2;
                
                if (worker.IsColonyMech || worker.RaceProps.Animal || 
                    RCDefOf.RC_ConfigCurves == null || 
                    !worker.IsHashIntervalTick(tickModulo)) return;
                
                RCLog.Message($"[RecipeDoRecipeWorkPostfix] Called on {worker.NameShortColored} during work.");
                
                int pawnsAvgSkillLevel = (int)worker.skills
                    .AverageOfRelevantSkillsFor(curJob.workGiverDef.workType);
                Building_WorkTable building = curJob.GetTarget(TargetIndex.A)
                    .Thing as Building_WorkTable;
                
                if (curJob.RecipeDef.defName.IndexOf("Cook", 
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!Rand.Chance(RCSettings.CookingFailureChance) || 
                        !Rand.Chance(RCDefOf.RC_ConfigCurves.cookingFailureCurve.Evaluate(pawnsAvgSkillLevel))) return;
                    if (Rand.Bool)
                    {
                        RCFailureUtil.StartFireHandler(worker, curJob, building);
                    }
                    else
                    {
                        RCFailureUtil.GiveInjuryHandler(worker, curJob, building);
                    }
                }
                else if (curJob.RecipeDef == RCDefOf.ButcherCorpseFlesh)
                {
                    if (!Rand.Chance(RCSettings.ButcheringFailureChance)) return;
                    if (Rand.Chance(RCDefOf.RC_ConfigCurves.butcheringMessCurve.Evaluate(pawnsAvgSkillLevel)))
                    {
                        RCFailureUtil.CauseMessHandler(worker, curJob, building);
                    }
                }
                else if (curJob.RecipeDef == RCDefOf.CremateCorpse)
                {
                    if (!Rand.Chance(RCSettings.CrematingInjuryChance)) return;
                    if (Rand.Chance(RCDefOf.RC_ConfigCurves.crematingInjuryCurve.Evaluate(pawnsAvgSkillLevel)))
                    {
                        RCFailureUtil.GiveInjuryHandler(worker, curJob, building);
                    }
                }
            });
        }
        
        public static void MakeRecipeProductsPrefix(ref RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
        {
            // because MO is busted as fuck -_-
            if (ModsConfig.IsActive("dankpyon.medieval.overhaul")) return;
            if (worker.IsColonyMech || worker.RaceProps.Animal || RCDefOf.RC_ConfigCurves == null) return;
            if (billGiver.GetWorkgiver() == null) return;
            
            int pawnsAvgSkillLevel = (int)worker.skills
                .AverageOfRelevantSkillsFor(billGiver
                    .GetWorkgiver().workType);

            if (!Rand.Chance(RCSettings.CookingBetterMealChance)) return;
            if (!RCFoodUtil.RecipeMap.TryGetValue(recipeDef, out RecipeDef newRecipeDef)) return;
            if (!Rand.Chance(RCDefOf.RC_ConfigCurves.betterMealOutcomeCurve.Evaluate(pawnsAvgSkillLevel))) return;
            
            recipeDef = newRecipeDef;

            if (RCSettings.AllowMessages)
            {
                Messages.Message("RC_BetterMealProduced".Translate(worker.Named("PAWN")),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        public static void MakeRecipeProductsPostfix(ref IEnumerable<Thing> __result, 
            RecipeDef recipeDef, Pawn worker, Thing dominantIngredient)
        {
            if (worker.IsColonyMech || worker.RaceProps.Animal || recipeDef == null) return;
            RandomProductExtension rpEx = recipeDef.GetModExtension<RandomProductExtension>();

            if (rpEx == null) return;
            if (worker.CurJob?.workGiverDef == null) return;

            float pawnsAvgSkillLevel = worker.skills
                .AverageOfRelevantSkillsFor(worker.CurJob.workGiverDef.workType);
            List<Thing> modifiedProducts = [];

            foreach (Thing product in __result)
            {
                bool productModified = false;

                if (rpEx.randomProductChance is > 0f && 
                    Rand.Chance(rpEx.randomProductChance.Value))
                {
                    if (!rpEx.randomProducts.NullOrEmpty())
                    {
                        float totalWeight = rpEx.randomProducts
                            .Sum(productData => productData.randomProductWeight);
                        float randomValue = Rand.Range(0f, totalWeight);
                        float accumulatedWeight = 0f;

                        foreach (RCRandomProductData productData in rpEx.randomProducts)
                        {
                            accumulatedWeight += productData.randomProductWeight;

                            if (!(randomValue <= accumulatedWeight)) continue;
                            ThingDef selectedDef = DefDatabase<ThingDef>
                                .GetNamed(productData.randomProduct.defName, errorOnFail: false);

                            if (selectedDef == null) continue;
                            // Cooking (bonus meals produced)
                            if (recipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                SimpleCurve bonusSpawnCurve = new()
                                {
                                    { 0, 0 },
                                    { 5, 0 },
                                    { 8, RCDefOf.RC_ConfigMisc.cookingBonusRangeOne.RandomInRange },
                                    { 14, RCDefOf.RC_ConfigMisc.cookingBonusRangeTwo.RandomInRange },
                                    { 18, RCDefOf.RC_ConfigMisc.cookingBonusRangeThree.RandomInRange },
                                    { 20, RCDefOf.RC_ConfigMisc.cookingBonusRangeFour.RandomInRange }
                                };

                                int bonusSpawnCount = (int)bonusSpawnCurve
                                    .Evaluate(pawnsAvgSkillLevel);
                                product.stackCount += bonusSpawnCount;

                                if (bonusSpawnCount > 0 && RCSettings.AllowMessages)
                                {
                                    Messages.Message("RC_BonusMealProduct"
                                        .Translate(worker.Named("PAWN"),
                                        product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                }
                                productModified = true;
                            }
                            else
                            {
                                // Stone cutting
                                Thing randomProduct = ThingMaker.MakeThing(selectedDef);
                                FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                                int skillBasedSpawnCount = Mathf
                                    .RoundToInt(pawnsAvgSkillLevel * 
                                                RCDefOf.RC_ConfigMisc.randomProductSkillsFactor);

                                FloatRange newMinSpawnCountRange = new(initialSpawnCountRange.min, 
                                    initialSpawnCountRange.min * skillBasedSpawnCount);
                                FloatRange newMaxSpawnCountRange = new(initialSpawnCountRange.max, 
                                    initialSpawnCountRange.max * skillBasedSpawnCount);

                                int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, 
                                    (int)newMinSpawnCountRange.max);
                                int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, 
                                    (int)newMaxSpawnCountRange.max);

                                randomProduct.stackCount = Rand.RangeInclusive(newMinSpawnCount, 
                                    newMaxSpawnCount);

                                if (RCSettings.AllowMessages)
                                {
                                    Messages.Message("RC_RandomStoneCuttingProduct"
                                        .Translate(worker.Named("PAWN"),
                                        dominantIngredient.Label, randomProduct.Label),
                                        worker, MessageTypeDefOf.PositiveEvent);
                                }
                                modifiedProducts.Add(randomProduct);
                                productModified = true;
                            }
                            break;
                        }
                    }
                }
                // Add the original product if it wasn't modified or if it was a cooking product
                if (!productModified || 
                    recipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    modifiedProducts.Add(product);
                }
            }
    
            // Additional check for butchering bonus products
            if (recipeDef == RCDefOf.ButcherCorpseFlesh && 
                Rand.Chance(RCSettings.BonusButcherProductChance))
            {
                Thing butcheredCorpse = worker.CurJob.GetTarget(TargetIndex.B).Thing;

                if (butcheredCorpse is Corpse corpse)
                {
                    if (corpse.InnerPawn.RaceProps.predator)
                    {
                        if (Rand.Chance(RCDefOf.RC_ConfigCurves
                                .butcherBonusProductsCurve.Evaluate(pawnsAvgSkillLevel)))
                        {
                            float butcheredCorpseBodySizeFactor = (corpse.InnerPawn.RaceProps.baseBodySize / 1.15f);
                            butcheredCorpseBodySizeFactor = Mathf.Max(butcheredCorpseBodySizeFactor, 1f);

                            ThingDef meatDef = RCFoodUtil.GetRandomMeatFromOtherPawn();
                            Thing additionalMeat = ThingMaker.MakeThing(meatDef);
                            additionalMeat.stackCount = RCDefOf.RC_ConfigMisc.additionalMeatStackCount.RandomInRange * 
                                                        Mathf.RoundToInt(butcheredCorpseBodySizeFactor);

                            if (RCSettings.AllowMessages)
                            {
                                Messages.Message("RC_PredatorButcheryBonusProduct"
                                    .Translate(worker.Named("PAWN"),
                                    additionalMeat.Label, dominantIngredient.Label),
                                    worker, MessageTypeDefOf.PositiveEvent);
                            }

                            modifiedProducts.Add(additionalMeat);
                        }
                    }
                }
            }
            __result = modifiedProducts;
        }
        
        public static void FixBrokenDownBuildingMakeNewToilsPostfix(
            ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
        {
            List<Toil> newToils = [..__result];
            int numToils = newToils.Count;
            
            Toil customToil = new()
            {
                initAction = delegate
                {
                    if (__instance.pawn.IsColonyMech || 
                        __instance.pawn.RaceProps.Animal || 
                        RCDefOf.RC_ConfigCurves == null) return;
                    if (__instance.job?.workGiverDef == null) return;
                    
                    float averageSkills = __instance.pawn.skills
                        .AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                    Building building = __instance.job
                        .GetTarget(TargetIndex.A).Thing as Building;

                    if (!Rand.Chance(RCSettings.ElectricalRepairFailureChance)) return;
                    
                    // stun the pawn and end the job!
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    int stunDur = RCDefOf.RC_ConfigMisc.repairFailureStunDuration;
                    __instance.pawn.stances.stunner
                        .StunFor(stunDur, __instance.pawn, false, false);
                    __instance.EndJobWith(JobCondition.Incompletable);
                    
                    if (building != null)
                    {
                        IntVec3 explosionCenter = building.Position;
                        Map explosionMap = building.Map;
                        int exRadius = RCDefOf.RC_ConfigMisc
                            .repairFailureExplosionRadius.RandomInRange;
                        int exDamage = RCDefOf.RC_ConfigMisc
                            .repairFailureExplosionDamageAmount.RandomInRange;
                        GenExplosion.DoExplosion(explosionCenter, explosionMap, 
                            exRadius, DamageDefOf.EMP, null, exDamage);
                        
                        if (Rand.Chance(RCSettings.ElectricalRepairFireChance))
                        {
                            GenExplosion.DoExplosion(explosionCenter, explosionMap, 
                                exRadius, DamageDefOf.Bomb, null, exDamage);
                            FireUtility.TryStartFireIn(building.Position, building.Map, 
                                RCDefOf.RC_ConfigMisc.repairFailureFireSize.RandomInRange, null);
                        }

                        if (RCSettings.AllowMessages)
                        {
                            Messages.Message("RC_ElectricalRepairFailure"
                                    .Translate(__instance.pawn.Named("PAWN")),
                                __instance.pawn, MessageTypeDefOf.NegativeEvent);
                        }

                        if (Rand.Chance(RCSettings.ElectricalRepairShortCircuitChance))
                        {
                            IncidentDef incident = RCDefOf.ShortCircuit;
                            IncidentParms parms = StorytellerUtility
                                .DefaultParmsNow(incident.category, building.Map);
                            incident.Worker.TryExecute(parms);
                        }
                        
                        // our injuries if needed
                        if (!Rand.Chance(RCDefOf.RC_ConfigCurves.brokenBuildingDamageCurve
                                .Evaluate(averageSkills))) return;
                        
                        BodyPartRecord bodyPart = __instance.pawn.RaceProps.body.corePart;
                        if (bodyPart == null) return;

                        foreach (HediffGiverSetDef hGSD in __instance.pawn.RaceProps.hediffGiverSets)
                        {
                            foreach (HediffGiver hG in hGSD.hediffGivers)
                            {
                                // check to see if the pawn can get the heart attack hediff
                                // (safeguarding for Killa's mechanical races)
                                if (hG.hediff is not { defName: "HeartAttack" }) continue;
                                HediffDef electricShock = RCDefOf.RC_ElectricShockHediff;
                                Hediff hediff = HediffMaker.MakeHediff(electricShock, __instance.pawn);
                                hediff.Severity = Rand.Value;
                                __instance.pawn.health.AddHediff(hediff, bodyPart);
                            
                                // our visuals if needed
                                Color fleckColor = new (
                                    RCDefOf.RC_ConfigMisc.shockFleckRChannel.RandomInRange,
                                    RCDefOf.RC_ConfigMisc.shockFleckGChannel.RandomInRange,
                                    RCDefOf.RC_ConfigMisc.shockFleckBChannel.RandomInRange);
                                
                                FleckCreationData fCD = FleckMaker.GetDataAttachedOverlay(__instance.pawn,
                                    RCDefOf.RC_ElectricShock, new Vector3(0.25f, 0f, 0.5f));
                                fCD.rotation = __instance.pawn.Drawer.renderer.BodyAngle(PawnRenderFlags.DrawNow);
                                fCD.instanceColor = fleckColor;
                                __instance.pawn.Map.flecks.CreateFleck(fCD);
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

        public static void GatherAnimalBodyResourcesMakeNewToilsPostfix(
            ref IEnumerable<Toil> __result, JobDriver_GatherAnimalBodyResources __instance)
        {
            List<Toil> newToils = [..__result];
            int numToils = newToils.Count;

            Toil customToil = new()
            {
                initAction = delegate
                {
                    if (__instance.pawn.IsColonyMech || 
                        __instance.pawn.RaceProps.Animal || 
                        RCDefOf.RC_ConfigCurves == null) return;
                    if (__instance.job?.workGiverDef == null) return;
                    
                    float pawnsAvgSkillLevel = __instance.pawn.skills
                        .AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                    if (!Rand.Chance(RCSettings.HurtByFarmAnimalChance) ||
                        !Rand.Chance(RCDefOf.RC_ConfigCurves.hurtByFarmAnimalCurve
                            .Evaluate(pawnsAvgSkillLevel))) return;
                    
                    if (__instance.job.GetTarget(TargetIndex.A)
                            .Thing is not Pawn animal) return;
                    List<Tool> tools = animal.Tools;

                    if (tools is not { Count: > 0 }) return;
                    Tool selectedTool = tools.RandomElement();

                    if (!Rand.Chance(selectedTool.chanceFactor)) return;
                    float damageAmount = Rand.Range(selectedTool.power / 2f, selectedTool.power);
                    DamageDef damageInflicted = selectedTool.Maneuvers
                        .RandomElement().verb.meleeDamageDef;
                    DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f);
                    __instance.pawn.TakeDamage(damageInfo);

                    if (damageAmount > 0f && RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_HurtByFarmAnimal"
                            .Translate(__instance.pawn.Named("PAWN"),
                            animal.NameShortColored), __instance.pawn, 
                            MessageTypeDefOf.NegativeEvent);
                    }

                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    int stunDur = RCDefOf.RC_ConfigMisc.farmAnimalInjuryStunDuration;
                    __instance.pawn.stances.stunner.StunFor(stunDur, 
                        __instance.pawn, false, false);
                    __instance.EndJobWith(JobCondition.Incompletable);
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
        
        public static void PlantWorkMakeNewToilsPostfix(
            ref IEnumerable<Toil> __result, JobDriver_PlantWork __instance)
        {
            List<Toil> newToils = [..__result];
            int numToils = newToils.Count;

            Toil customToil = new()
            {
                initAction = delegate
                {
                    Map map = __instance.job.GetTarget(TargetIndex.A).Thing.Map;

                    if (RCDefOf.RC_ConfigCurves == null || map == null || 
                        __instance.pawn.IsColonyMech || 
                        __instance.pawn.RaceProps.Animal || 
                        __instance.pawn.IsWildMan()) return;
                    if (__instance.job?.workGiverDef == null) return;
                    
                    MapComponent_CollectThings thingCollections = 
                        map.GetComponent<MapComponent_CollectThings>();
                    float pawnsAvgSkillLevel = __instance.pawn.skills
                        .AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                    ThingDef chosenEggDef = thingCollections?.PossibleEggs.RandomElement();

                    if (chosenEggDef?.GetCompProperties<CompProperties_Hatcher>() == null) return;
                    if (!Rand.Chance(RCSettings.PlantHarvestingFindEggsChance) ||
                        !Rand.Chance(RCDefOf.RC_ConfigCurves.plantWorkDiscoveryCurve
                            .Evaluate(pawnsAvgSkillLevel))) return;
                    
                    Thing eggs = ThingMaker.MakeThing(chosenEggDef);
                    eggs.stackCount = RCDefOf.RC_ConfigMisc.plantWorkEggDiscoveryCount.RandomInRange;
                    GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_PlantWorkFoundEggs"
                            .Translate(__instance.pawn.Named("PAWN"),
                            eggs.Label), __instance.pawn, 
                            MessageTypeDefOf.PositiveEvent);
                    }
                    
                    Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;

                    if (!Rand.Chance(RCSettings.PlantHarvestAgitatedWildAnimalChance) && 
                        !Rand.Chance(RCDefOf.RC_ConfigCurves.agitatedWildAnimalCurve
                            .Evaluate(pawnsAvgSkillLevel))) return;
                    
                    PawnKindDef agitatedAnimalKind = chosenEggDef
                        .GetCompProperties<CompProperties_Hatcher>().hatcherPawn;

                    if (targetThing.def.plant.sowTags.Contains("Hydroponic") && 
                        targetThing.Position.Roofed(map)) return;
                    
                    IntVec3 spawnCell = CellFinder
                        .RandomClosewalkCellNear(__instance.pawn.Position, map, 1);
                    Pawn agitatedAnimal = PawnGenerator
                        .GeneratePawn(agitatedAnimalKind, null);
                    agitatedAnimal.gender = Gender.Female;
                    GenSpawn.Spawn(agitatedAnimal, spawnCell, map);
                    agitatedAnimal.mindState?.mentalStateHandler?
                        .TryStartMentalState(MentalStateDefOf.Manhunter);

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_PlantWorkAngryMomma"
                            .Translate(__instance.pawn.Named("PAWN"),
                            agitatedAnimal.Label), __instance.pawn,
                            MessageTypeDefOf.NegativeEvent);
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }

        public static void WearMakeNewToilsPostfix(ref IEnumerable<Toil> __result, 
            JobDriver_Wear __instance)
        {
            List<Toil> newToils = [..__result];
            int numToils = newToils.Count;

            Toil customToil = new()
            {
                initAction = delegate
                {
                    if (__instance.pawn == null || 
                        __instance.job == null || 
                        __instance.pawn.skills == null) return;
                    
                    if (__instance.pawn.IsColonyMech || 
                        __instance.pawn.RaceProps.Animal || 
                        RCDefOf.RC_ConfigCurves == null) return;

                    DamageDef damageInflicted = DamageDefOf.Cut;
                    SimpleCurve injuryCurve = RCDefOf.RC_ConfigCurves.powerArmorInjuryCurve;
                    if (injuryCurve == null) return;

                    float pawnsSkillLevel = __instance.pawn.skills
                        .GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
                    if (pawnsSkillLevel == 0) return;

                    Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;
                    CompQuality compQuality = targetThing?.TryGetComp<CompQuality>();
                    if (compQuality == null) return;

                    float qualityFactor = RCQualityUtil
                        .GetQualityValue(compQuality.Quality, injuryCurve);
                    List<BodyPartRecord> digits = __instance.pawn.RaceProps.body
                        .GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit);
                    ThingDef apparelPiece = targetThing.def;
                    if (digits == null || digits.Count == 0) return;
                    if (!Rand.Chance(RCSettings.InjuredByApparelChance) ||
                        !Rand.Chance(injuryCurve.Evaluate(pawnsSkillLevel))) return;

                    float damageAmount = Rand.Range(0.1f, pawnsSkillLevel) * qualityFactor;
                    if (apparelPiece == RCDefOf.Apparel_PowerArmor) // body
                    {
                        DamageInfo damageInfo = new(damageInflicted, damageAmount, 
                            1f, -1f, null, digits.RandomElement());
                        __instance.pawn.TakeDamage(damageInfo);
                    }
                    else if (apparelPiece == RCDefOf.Apparel_PowerArmorHelmet) // head
                    {
                        BodyPartRecord headPart = __instance.pawn.RaceProps.body
                            .GetPartsWithDef(BodyPartDefOf.Head).RandomElement();
                        if (headPart == null) return;

                        DamageInfo damageInfo = new(damageInflicted, damageAmount,
                            1f, -1f, null, headPart);
                        __instance.pawn.TakeDamage(damageInfo);
                    }

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryWhileDonningArmor"
                            .Translate(__instance.pawn.Named("PAWN"),
                            apparelPiece.label), __instance.pawn, 
                            MessageTypeDefOf.NegativeEvent);
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }

        public static void TrySpawnYieldPostFix(ref Map map, bool moteOnWaste, 
            Pawn pawn, Mineable __instance)
        {
            if (DebugSettings.godMode || Prefs.DevMode) return;
            if (map == null || __instance?.def?.building == null) return;

            FieldInfo yieldPctField = AccessTools.Field(typeof(Mineable), "yieldPct");
            float yieldPct = (float)yieldPctField.GetValue(__instance);

            // Spawn the original mineableThing
            if (__instance.def.building.mineableThing != null && 
                !(Rand.Value > __instance.def.building.mineableDropChance))
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

            if (pawn.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
            if (pawn.CurJob?.workGiverDef == null) return;
            
            RandomProductExtension rpEx = __instance.def
                .GetModExtension<RandomProductExtension>();
            int pawnsAvgSkillLevel = (int)pawn.skills
                .AverageOfRelevantSkillsFor(pawn.CurJob.workGiverDef.workType);
            SimpleCurve extraYieldCurve = RCDefOf.RC_ConfigCurves.extraMiningYieldCurve;

            if (rpEx == null || rpEx.randomProducts == null || 
                !rpEx.randomProductChance.HasValue ||
                !(rpEx.randomProductChance.Value > 0f) || 
                !Rand.Chance(rpEx.randomProductChance.Value)) return;
            if (extraYieldCurve == null) return;
            if (rpEx.randomProducts == null) return;
            
            float totalWeight = rpEx.randomProducts
                .Sum(productData => productData?.randomProductWeight ?? 0f);

            if (!(totalWeight > 0f)) return;
            {
                float randomValue = Rand.Range(0f, totalWeight);
                float accumulatedWeight = 0f;

                foreach (RCRandomProductData productData in rpEx.randomProducts)
                {
                    if (productData == null)
                        continue;

                    accumulatedWeight += productData.randomProductWeight;

                    if (!(randomValue <= accumulatedWeight)) continue;
                    
                    ThingDef selectedDef = DefDatabase<ThingDef>
                        .GetNamed(productData.randomProduct?.defName, errorOnFail: false);

                    if (selectedDef != null)
                    {
                        Thing thingToSpawn = ThingMaker.MakeThing(selectedDef);

                        FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                        int skillBasedSpawnCount = Mathf.RoundToInt(
                            pawnsAvgSkillLevel * RCDefOf.RC_ConfigMisc.miningExtraProductSkillsFactor);

                        FloatRange newMinSpawnCountRange = new(initialSpawnCountRange.min, 
                            initialSpawnCountRange.min * skillBasedSpawnCount);
                        FloatRange newMaxSpawnCountRange = new(initialSpawnCountRange.max, 
                            initialSpawnCountRange.max * skillBasedSpawnCount);

                        int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, 
                            (int)newMinSpawnCountRange.max);
                        int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, 
                            (int)newMaxSpawnCountRange.max);

                        thingToSpawn.stackCount = Rand.RangeInclusive(newMinSpawnCount, 
                            newMaxSpawnCount);
                        GenPlace.TryPlaceThing(thingToSpawn, __instance.Position, 
                            map, ThingPlaceMode.Near);
                    }
                    break; // Exit the loop after selecting one item.
                }
            }
        }

        public static void ViewArtWaitTickActionPrefix(JobDriver_ViewArt __instance)
        {
            float negativeMoodChance = 0.25f;
            float positiveMoodChance = 0.25f;
            Thing artPiece = __instance.job.GetTarget(TargetIndex.A).Thing;
            Pawn pawn = __instance.pawn;
            
            if (artPiece == null) return;
            if (!artPiece.TryGetQuality(out QualityCategory quality)) return;
            
            if (quality == QualityCategory.Awful && Rand.Chance(negativeMoodChance))
            {
                pawn.needs.mood.thoughts.memories
                    .TryGainMemory(RCDefOf.RC_ViewedInsultingArtWork);
            }
            
            switch (quality)
            {
                case QualityCategory.Awful:
                    if (Rand.Chance(negativeMoodChance))
                        pawn.needs.mood.thoughts.memories
                            .TryGainMemory(RCDefOf.RC_ViewedAwfulArtWork);
                    break;
                case QualityCategory.Poor:
                    if (Rand.Chance(negativeMoodChance))
                        pawn.needs.mood.thoughts.memories
                            .TryGainMemory(RCDefOf.RC_ViewedPoorArtWork);
                    break;
                case QualityCategory.Masterwork:
                    if (Rand.Chance(positiveMoodChance))
                        pawn.needs.mood.thoughts.memories
                            .TryGainMemory(RCDefOf.RC_ViewedMasterworkArtWork);
                    break;
                case QualityCategory.Legendary:
                    if (Rand.Chance(positiveMoodChance))
                        pawn.needs.mood.thoughts.memories
                            .TryGainMemory(RCDefOf.RC_ViewedLegendaryArtWork);
                    break;
            }
        }
        
        public static void RemoveBuildingMakeNewToilsPostFix(
            ref IEnumerable<Toil> __result, JobDriver_RemoveBuilding __instance)
        {
            Map map = __instance.pawn?.Map;
            MapComponent_TimeKeeping timeKeeper = map?
                .GetComponent<MapComponent_TimeKeeping>();
            
            if (timeKeeper == null) return;
            if (timeKeeper.LastSilverFindTick < timeKeeper.SilverFindThreshold) return;
            timeKeeper.LastSilverFindTick = 0;
            
            List<Toil> newToils = [..__result];
            
            Toil customToil = new()
            {
                initAction = delegate
                {
                    if (RCDefOf.RC_ConfigMisc == null || 
                        RCDefOf.RC_ConfigCurves == null) return;
                    if (__instance.pawn == null || __instance.job == null) return;
                    
                    Thing building = __instance.job.targetA.Thing;
                    
                    if (building?.Map == null) return;
                    if (__instance.pawn == null || __instance.job == null) return;
                    
                    if (!Rand.Chance(RCDefOf.RC_ConfigMisc.buildingRemovalSilverFindChance))
                    {
                        Thing thingToSpawn = ThingMaker.MakeThing(ThingDefOf.Silver);
                        thingToSpawn.stackCount = RCDefOf.RC_ConfigMisc
                            .buildingRemovalSilverFindCount.RandomInRange;
                        GenPlace.TryPlaceThing(thingToSpawn, building.Position, 
                            building.Map, ThingPlaceMode.Near);
                    }
                    else
                    {
                        ThingDef filth = HarmonyPatchesUtil.FilthOptionsCache.RandomElement();
                        FilthMaker.TryMakeFilth(building.Position, building.Map, filth);
                    }
                }
            };
            
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
    }
}