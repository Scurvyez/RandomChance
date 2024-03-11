﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch(typeof(JobDriver_PlantWork), "MakeNewToils")]
    public class JobDriverPlantWorkMakeNewToils_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_PlantWork __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        float findAnimalEggsChance = RandomChanceSettings.PlantHarvestingFindEggsChance; // 5% by default
                        SimpleCurve discoveryCurve = RandomChance_DefOf.RC_Curves.plantWorkDiscoveryCurve;
                        SimpleCurve aggitatedAnimalCurve = RandomChance_DefOf.RC_Curves.aggitatedWildAnimalCurve;

                        if (Rand.Chance(findAnimalEggsChance))
                        {
                            List<PawnKindDef> eggLayingAnimals = new();
                            Map map = __instance.job.GetTarget(TargetIndex.A).Thing.Map;
                            FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");
                            float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                            if (Rand.Chance(discoveryCurve.Evaluate(pawnsAvgSkillLevel)))
                            {
                                if (map != null)
                                {
                                    if (wildAnimalsField.GetValue(map.Biome) is List<BiomeAnimalRecord> biomeSpecificAnimals)
                                    {
                                        foreach (BiomeAnimalRecord animalRecord in biomeSpecificAnimals)
                                        {
                                            PawnKindDef kindDef = animalRecord.animal;
                                            if (kindDef != null && kindDef.race.GetCompProperties<CompProperties_EggLayer>() != null)
                                            {
                                                eggLayingAnimals.Add(kindDef);
                                            }
                                        }
                                    }

                                    List<ThingDef> possibleEggs = new();
                                    for (int i = 0; i < eggLayingAnimals.Count; i++)
                                    {
                                        PawnKindDef kindDef = eggLayingAnimals[i];
                                        Pawn pawn = PawnGenerator.GeneratePawn(kindDef, null);
                                        CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
                                        if (compEggLayer != null)
                                        {
                                            possibleEggs.Add(compEggLayer.Props.eggUnfertilizedDef);
                                            possibleEggs.Add(compEggLayer.Props.eggFertilizedDef);
                                        }
                                    }

                                    if (possibleEggs.NullOrEmpty())
                                        return;

                                    ThingDef chosenEggDef = possibleEggs.RandomElement();
                                    if (chosenEggDef != null)
                                    {
                                        Thing eggs = ThingMaker.MakeThing(chosenEggDef, null);
                                        eggs.stackCount = Rand.RangeInclusive(1, 3);
                                        GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);

                                        if (RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_PlantHarvestingFoundEggs".Translate(__instance.pawn.Named("PAWN"),
                                                eggs.Label), __instance.pawn, MessageTypeDefOf.PositiveEvent);
                                        }

                                        float pawnAnimalSkill = __instance.pawn.skills.GetSkill(SkillDefOf.Animals).Level;
                                        if (Rand.Chance(aggitatedAnimalCurve.Evaluate(pawnAnimalSkill)))
                                        {
                                            PawnKindDef agitatedAnimalKind = chosenEggDef.GetCompProperties<CompProperties_Hatcher>().hatcherPawn;
                                            Pawn agitatedAnimal = PawnGenerator.GeneratePawn(agitatedAnimalKind, null);
                                            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(__instance.pawn.Position, map, 1);
                                            GenSpawn.Spawn(agitatedAnimal, spawnCell, map);
                                            agitatedAnimal.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Manhunter);
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
    }
}
