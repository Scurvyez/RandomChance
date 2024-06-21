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
#pragma warning disable CS0642 // Possible mistaken empty statement

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.scurvyez.randomchance");

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

            harmony.Patch(original: AccessTools.Method(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(TrySpawnYieldPostFix)));

            harmony.Patch(original: AccessTools.Method(typeof(JobDriver_ViewArt), "WaitTickAction"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ViewArtWaitTickActionPrefix)));
        }
        
        public static void MakeRecipeProductsPrefix(ref RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
        {
            if (worker.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
            
            int pawnsAvgSkillLevel = (int)worker.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);

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

        public static void MakeRecipeProductsPostfix(ref IEnumerable<Thing> __result, RecipeDef recipeDef, Pawn worker, Thing dominantIngredient)
        {
            if (worker.IsColonyMech || recipeDef == null) return;
            RandomProductExtension rpEx = recipeDef.GetModExtension<RandomProductExtension>();

            if (rpEx == null) return;
            {
                float pawnsAvgSkillLevel = worker.skills.AverageOfRelevantSkillsFor(worker.CurJob.workGiverDef.workType);
                List<Thing> modifiedProducts = new();

                foreach (Thing product in __result)
                {
                    if (rpEx.randomProductChance.HasValue && rpEx.randomProductChance.Value > 0f && Rand.Chance(rpEx.randomProductChance.Value));
                    {
                        if (!rpEx.randomProducts.NullOrEmpty())
                        {
                            float totalWeight = 0f;
                            foreach (RCRandomProductData productData in rpEx.randomProducts)
                            {
                                totalWeight += productData.randomProductWeight;
                            }

                            float randomValue = Rand.Range(0f, totalWeight);
                            float accumulatedWeight = 0f;

                            foreach (RCRandomProductData productData in rpEx.randomProducts)
                            {
                                accumulatedWeight += productData.randomProductWeight;

                                if (!(randomValue <= accumulatedWeight)) continue;
                            
                                ThingDef selectedDef = DefDatabase<ThingDef>.GetNamed(productData.randomProduct.defName, errorOnFail: false);
                                if (selectedDef != null)
                                {
                                    if (recipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        foreach (RCRandomProductData rP in rpEx.randomProducts)
                                        {
                                            if (product.def != rP.randomProduct) continue;
                                            product.def = rP.randomProduct;
                                            SimpleCurve bonusSpawnCurve = new()
                                            {
                                                { 0, 0 },
                                                { 5, 0 },
                                                { 8, RCDefOf.RC_ConfigMisc.cookingBonusRangeOne.RandomInRange },
                                                { 14, RCDefOf.RC_ConfigMisc.cookingBonusRangeTwo.RandomInRange },
                                                { 18, RCDefOf.RC_ConfigMisc.cookingBonusRangeThree.RandomInRange },
                                                { 20, RCDefOf.RC_ConfigMisc.cookingBonusRangeFour.RandomInRange }
                                            };

                                            int bonusSpawnCount = (int)bonusSpawnCurve.Evaluate(pawnsAvgSkillLevel);
                                            int finalSpawnCount = product.stackCount + bonusSpawnCount;
                                            product.stackCount = finalSpawnCount;

                                            if (bonusSpawnCount > 0 && RCSettings.AllowMessages)
                                            {
                                                Messages.Message("RC_BonusMealProduct".Translate(worker.Named("PAWN"),
                                                    product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        product.def = selectedDef;
                                        FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                                        int skillBasedSpawnCount = Mathf.RoundToInt(pawnsAvgSkillLevel * RCDefOf.RC_ConfigMisc.randomProductSkillsFactor);

                                        FloatRange newMinSpawnCountRange = new(initialSpawnCountRange.min, initialSpawnCountRange.min * skillBasedSpawnCount);
                                        FloatRange newMaxSpawnCountRange = new(initialSpawnCountRange.max, initialSpawnCountRange.max * skillBasedSpawnCount);

                                        int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, (int)newMinSpawnCountRange.max);
                                        int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, (int)newMaxSpawnCountRange.max);

                                        product.stackCount = Rand.RangeInclusive(newMinSpawnCount, newMaxSpawnCount);

                                        if (RCSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_RandomStoneCuttingProduct".Translate(worker.Named("PAWN"),
                                                dominantIngredient.Label, product.Label), worker, MessageTypeDefOf.PositiveEvent);
                                        }
                                    }
                                }
                                break;
                            }
                        }
                        else
                        {
                            if (recipeDef == RCDefOf.ButcherCorpseFlesh && Rand.Chance(RCSettings.BonusButcherProductChance))
                            {
                                Thing butcheredCorpse = worker.CurJob.GetTarget(TargetIndex.B).Thing;
                        
                                if (butcheredCorpse is Corpse corpse)
                                {
                                    if (corpse.InnerPawn.RaceProps.predator)
                                    {
                                        if (Rand.Chance(RCDefOf.RC_ConfigCurves.butcherBonusProductsCurve.Evaluate(pawnsAvgSkillLevel)))
                                        {
                                            float butcheredCorpseBodySizeFactor = (corpse.InnerPawn.RaceProps.baseBodySize / 1.15f);
                                            butcheredCorpseBodySizeFactor = Mathf.Max(butcheredCorpseBodySizeFactor, 1f);

                                            ThingDef meatDef = RCFoodUtil.GetRandomMeatFromOtherPawn();
                                            Thing additionalMeat = ThingMaker.MakeThing(meatDef);
                                            additionalMeat.stackCount = RCDefOf.RC_ConfigMisc.additionalMeatStackCount.RandomInRange * Mathf.RoundToInt(butcheredCorpseBodySizeFactor);

                                            if (RCSettings.AllowMessages)
                                            {
                                                Messages.Message("RC_PredatorButcheryBonusProduct".Translate(worker.Named("PAWN"),
                                                    additionalMeat.Label, dominantIngredient.Label), worker, MessageTypeDefOf.PositiveEvent);
                                            }

                                            modifiedProducts.Add(additionalMeat);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    modifiedProducts.Add(product);
                }
                __result = modifiedProducts;
            }
        }

        public static void FixBrokenDownBuildingMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (__instance.pawn.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
                    
                    float averageSkills = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                    Building building = __instance.job.GetTarget(TargetIndex.A).Thing as Building;

                    if (!Rand.Chance(RCSettings.ElectricalRepairFailureChance)) return;
                    
                    // stun the pawn and end the job!
                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    int stunDur = RCDefOf.RC_ConfigMisc.repairFailureStunDuration;
                    __instance.pawn.stances.stunner.StunFor(stunDur, __instance.pawn, false, false);
                    __instance.EndJobWith(JobCondition.Incompletable);
                    
                    if (building != null)
                    {
                        IntVec3 explosionCenter = building.Position;
                        Map explosionMap = building.Map;
                        int exRadius = RCDefOf.RC_ConfigMisc.repairFailureExplosionRadius.RandomInRange;
                        int exDamage = RCDefOf.RC_ConfigMisc.repairFailureExpolsionDamageAmount.RandomInRange;
                        GenExplosion.DoExplosion(explosionCenter, explosionMap, exRadius, DamageDefOf.EMP, null, exDamage);
                        
                        if (Rand.Chance(RCSettings.ElectricalRepairFireChance))
                        {
                            GenExplosion.DoExplosion(explosionCenter, explosionMap, exRadius, DamageDefOf.Bomb, null, exDamage);
                            FireUtility.TryStartFireIn(building.Position, building.Map, RCDefOf.RC_ConfigMisc.repairFailureFireSize.RandomInRange, null);
                        }

                        if (RCSettings.AllowMessages)
                        {
                            Messages.Message("RC_ElectricalRepairFailure".Translate(__instance.pawn.Named("PAWN")),
                                __instance.pawn, MessageTypeDefOf.NegativeEvent);
                        }

                        if (Rand.Chance(RCSettings.ElectricalRepairShortCircuitChance))
                        {
                            IncidentDef incident = RCDefOf.ShortCircuit;
                            IncidentParms parms = StorytellerUtility.DefaultParmsNow(incident.category, building.Map);
                            incident.Worker.TryExecute(parms);
                        }
                        
                        // our injuries if needed
                        if (!Rand.Chance(RCDefOf.RC_ConfigCurves.brokenBuildingDamageCurve.Evaluate(averageSkills))) return;
                        BodyPartRecord bodyPart = __instance.pawn.RaceProps.body.corePart;
                        if (bodyPart == null) return;

                        foreach (HediffGiverSetDef hGSD in __instance.pawn.RaceProps.hediffGiverSets)
                        {
                            foreach (HediffGiver hG in hGSD.hediffGivers)
                            {
                                // check to see if the pawn can get the heart attack hediff (safeguarding for Killa's mechanical races)
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

        public static void GatherAnimalBodyResourcesMakeNewToilsPostfix(ref IEnumerable<Toil> __result, JobDriver_GatherAnimalBodyResources __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (__instance.pawn.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
                    
                    float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                    if (!Rand.Chance(RCSettings.HurtByFarmAnimalChance) ||
                        !Rand.Chance(RCDefOf.RC_ConfigCurves.hurtByFarmAnimalCurve.Evaluate(pawnsAvgSkillLevel))) return;
                    if (__instance.job.GetTarget(TargetIndex.A).Thing is not Pawn animal) return;
                    if (animal == null) return;
                    
                    List<Tool> tools = animal.Tools;

                    if (tools == null || tools.Count <= 0) return;
                    
                    Tool selectedTool = tools.RandomElement();

                    if (!Rand.Chance(selectedTool.chanceFactor) || selectedTool == null) return;
                    
                    float damageAmount = Rand.Range(selectedTool.power / 2f, selectedTool.power);
                    DamageDef damageInflicted = selectedTool.Maneuvers.RandomElement().verb.meleeDamageDef;
                    DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f);
                    __instance.pawn.TakeDamage(damageInfo);

                    if (damageAmount > 0f && RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_HurtByFarmAnimal".Translate(__instance.pawn.Named("PAWN"),
                            animal.NameShortColored), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                    }

                    Find.TickManager.slower.SignalForceNormalSpeedShort();
                    int stunDur = RCDefOf.RC_ConfigMisc.farmAnimalInjuryStunDuration;
                    __instance.pawn.stances.stunner.StunFor(stunDur, __instance.pawn, false, false);
                    __instance.EndJobWith(JobCondition.Incompletable);
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

                    if (RCDefOf.RC_ConfigCurves == null || map == null || __instance.pawn.IsColonyMech) return;
                    
                    MapComponent_CollectThings thingCollections = map.GetComponent<MapComponent_CollectThings>();
                    float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                    if (thingCollections == null) return;
                    
                    ThingDef chosenEggDef = thingCollections.possibleEggs.RandomElement();

                    if (chosenEggDef?.GetCompProperties<CompProperties_Hatcher>() == null) return;
                    if (Rand.Chance(RCSettings.PlantHarvestingFindEggsChance) && Rand.Chance(RCDefOf.RC_ConfigCurves.plantWorkDiscoveryCurve.Evaluate(pawnsAvgSkillLevel)))
                    {
                        Thing eggs = ThingMaker.MakeThing(chosenEggDef, null);
                        eggs.stackCount = RCDefOf.RC_ConfigMisc.plantWorkEggDiscoveryCount.RandomInRange;
                        GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);

                        if (RCSettings.AllowMessages)
                        {
                            Messages.Message("RC_PlantWorkFoundEggs".Translate(__instance.pawn.Named("PAWN"),
                                eggs.Label), __instance.pawn, MessageTypeDefOf.PositiveEvent);
                        }
                    }

                    Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;

                    if (!Rand.Chance(RCDefOf.RC_ConfigCurves.agitatedWildAnimalCurve.Evaluate(pawnsAvgSkillLevel))) return;
                    
                    PawnKindDef agitatedAnimalKind = chosenEggDef.GetCompProperties<CompProperties_Hatcher>().hatcherPawn;

                    if (targetThing.def.plant.sowTags.Contains("Hydroponic") &&
                        targetThing.Position.Roofed(map)) return;
                    
                    IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(__instance.pawn.Position, map, 1);
                    Pawn agitatedAnimal = PawnGenerator.GeneratePawn(agitatedAnimalKind, null);
                    agitatedAnimal.gender = Gender.Female;
                    GenSpawn.Spawn(agitatedAnimal, spawnCell, map);
                    agitatedAnimal.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Manhunter);

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_PlantWorkAngryMomma".Translate(__instance.pawn.Named("PAWN"),
                            agitatedAnimal.Label), __instance.pawn, MessageTypeDefOf.NegativeEvent);
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
                    if (__instance.pawn.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
                    
                    DamageDef damageInflicted = DamageDefOf.Cut;
                    SimpleCurve injuryCurve = RCDefOf.RC_ConfigCurves.powerArmorInjuryCurve;
                    float pawnsSkillLevel = __instance.pawn.skills.GetSkill(SkillDefOf.Intellectual).Level; // check if the Pawn is dumb as shit lol
                    float qualityFactor = RCQualityUtil.GetQualityValue(__instance.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompQuality>().Quality, injuryCurve);
                    List<BodyPartRecord> digits = __instance.pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit);
                    ThingDef apparelPiece = __instance.job.GetTarget(TargetIndex.A).Thing.def;

                    if (!Rand.Chance(RCSettings.InjuredByApparelChance) ||
                        !Rand.Chance(injuryCurve.Evaluate(pawnsSkillLevel))) return;
                    
                    float damageAmount = Rand.Range(0.1f, pawnsSkillLevel) * qualityFactor;

                    if (apparelPiece == RCDefOf.Apparel_PowerArmor) // body
                    {
                        DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, digits.RandomElement());
                        __instance.pawn.TakeDamage(damageInfo);
                    }
                    else if (apparelPiece == RCDefOf.Apparel_PowerArmorHelmet) // head
                    {
                        DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, __instance.pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head).RandomElement());
                        __instance.pawn.TakeDamage(damageInfo);
                    }

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryWhileDonningArmor".Translate(__instance.pawn.Named("PAWN"),
                            apparelPiece.label), __instance.pawn, MessageTypeDefOf.NegativeEvent);
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
            if (DebugSettings.godMode || Prefs.DevMode) return;
            if (map == null || __instance?.def?.building == null) return;
            //RCLog.Message($"TrySpawnYieldPostfix called for job on: {__instance.def.label}");

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

            if (pawn.IsColonyMech || RCDefOf.RC_ConfigCurves == null) return;
            
            RandomProductExtension rpEx = __instance.def.GetModExtension<RandomProductExtension>();
            int pawnsAvgSkillLevel = (int)pawn.skills.AverageOfRelevantSkillsFor(pawn.CurJob.workGiverDef.workType);
            SimpleCurve extraYieldCurve = RCDefOf.RC_ConfigCurves.extraMiningYieldCurve;

            if (rpEx == null || rpEx.randomProducts == null || !rpEx.randomProductChance.HasValue ||
                !(rpEx.randomProductChance.Value > 0f)
                || !Rand.Chance(rpEx.randomProductChance.Value)) return;
            if (extraYieldCurve == null) return;
            if (rpEx.randomProducts == null) return;
            
            float totalWeight = rpEx.randomProducts.Sum(productData => productData?.randomProductWeight ?? 0f);

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
                    
                    ThingDef selectedDef = DefDatabase<ThingDef>.GetNamed(productData.randomProduct?.defName, errorOnFail: false);

                    if (selectedDef != null)
                    {
                        Thing thingToSpawn = ThingMaker.MakeThing(selectedDef);

                        FloatRange initialSpawnCountRange = productData.randomProductAmountRange;

                        int skillBasedSpawnCount = Mathf.RoundToInt(pawnsAvgSkillLevel * RCDefOf.RC_ConfigMisc.miningExtraProductSkillsFactor);

                        FloatRange newMinSpawnCountRange = new(initialSpawnCountRange.min, initialSpawnCountRange.min * skillBasedSpawnCount);
                        FloatRange newMaxSpawnCountRange = new(initialSpawnCountRange.max, initialSpawnCountRange.max * skillBasedSpawnCount);

                        int newMinSpawnCount = Rand.RangeInclusive((int)newMinSpawnCountRange.min, (int)newMinSpawnCountRange.max);
                        int newMaxSpawnCount = Rand.RangeInclusive((int)newMaxSpawnCountRange.min, (int)newMaxSpawnCountRange.max);

                        thingToSpawn.stackCount = Rand.RangeInclusive(newMinSpawnCount, newMaxSpawnCount);
                        GenPlace.TryPlaceThing(thingToSpawn, __instance.Position, map, ThingPlaceMode.Near);
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
                pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ViewedInsultingArtWork);
            }
            
            switch (quality)
            {
                case QualityCategory.Awful:
                    if (Rand.Chance(negativeMoodChance))
                        pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ViewedAwfulArtWork);
                    break;
                case QualityCategory.Poor:
                    if (Rand.Chance(negativeMoodChance))
                        pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ViewedPoorArtWork);
                    break;
                case QualityCategory.Masterwork:
                    if (Rand.Chance(positiveMoodChance))
                        pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ViewedMasterworkArtWork);
                    break;
                case QualityCategory.Legendary:
                    if (Rand.Chance(positiveMoodChance))
                        pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ViewedLegendaryArtWork);
                    break;
            }
        }
    }
}
