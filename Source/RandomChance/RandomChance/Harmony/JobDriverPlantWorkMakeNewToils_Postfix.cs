using HarmonyLib;
using RandomChance.MapComps;
using RimWorld;
using System.Collections.Generic;
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
            Map map = __instance.job.GetTarget(TargetIndex.A).Thing.Map;
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (map != null && !__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        MapComponent_CollectThings thingCollections = map.GetComponent<MapComponent_CollectThings>();

                        float findAnimalEggsChance = RandomChanceSettings.PlantHarvestingFindEggsChance; // 5% by default
                        SimpleCurve discoveryCurve = RandomChance_DefOf.RC_Curves.plantWorkDiscoveryCurve;
                        float pawnsAvgSkillLevel = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);

                        if (thingCollections != null)
                        {
                            ThingDef chosenEggDef = thingCollections.possibleEggs.RandomElement();

                            if (chosenEggDef != null && chosenEggDef.GetCompProperties<CompProperties_Hatcher>() != null)
                            {
                                if (Rand.Chance(findAnimalEggsChance) && Rand.Chance(discoveryCurve.Evaluate(pawnsAvgSkillLevel)))
                                {
                                    if (chosenEggDef != null && chosenEggDef.GetCompProperties<CompProperties_Hatcher>() != null)
                                    {
                                        Thing eggs = ThingMaker.MakeThing(chosenEggDef, null);
                                        eggs.stackCount = Rand.RangeInclusive(1, 3);
                                        GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);

                                        if (RandomChanceSettings.AllowMessages)
                                        {
                                            Messages.Message("RC_PlantHarvestingFoundEggs".Translate(__instance.pawn.Named("PAWN"),
                                                eggs.Label), __instance.pawn, MessageTypeDefOf.PositiveEvent);
                                        }
                                    }
                                }

                                Thing targetThing = __instance.job.GetTarget(TargetIndex.A).Thing;
                                SimpleCurve agitatedAnimalCurve = RandomChance_DefOf.RC_Curves.agitatedWildAnimalCurve;
                                if (Rand.Chance(agitatedAnimalCurve.Evaluate(pawnsAvgSkillLevel)))
                                {
                                    PawnKindDef agitatedAnimalKind = chosenEggDef.GetCompProperties<CompProperties_Hatcher>().hatcherPawn;

                                    if (!targetThing.def.plant.sowTags.Contains("Hydroponic"))
                                    {
                                        IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(__instance.pawn.Position, map, 1);
                                        Pawn agitatedAnimal = PawnGenerator.GeneratePawn(agitatedAnimalKind, null);
                                        GenSpawn.Spawn(agitatedAnimal, spawnCell, map);
                                        agitatedAnimal.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.Manhunter);
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
