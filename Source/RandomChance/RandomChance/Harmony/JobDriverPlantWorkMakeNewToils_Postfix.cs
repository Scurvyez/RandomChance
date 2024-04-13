using HarmonyLib;
using RandomChance.MapComps;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RandomChance
{
    /*
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
    }
    */
}
