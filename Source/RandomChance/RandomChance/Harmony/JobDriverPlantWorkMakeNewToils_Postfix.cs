using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
                    Map map = __instance.job.GetTarget(TargetIndex.A).Thing.Map;
                    float averageSkills = __instance.pawn.skills.AverageOfRelevantSkillsFor(__instance.job.workGiverDef.workType);
                    FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");
                    List<BiomeAnimalRecord> biomeSpecificAnimals = wildAnimalsField.GetValue(map.Biome) as List<BiomeAnimalRecord>;
                    List<PawnKindDef> eggLayingAnimals = new();
                    //float findAnimalEggsChance = RandomChanceSettings.FindAnimalEggsChance; // 3% by default

                    if (Rand.Chance(0.75f)) 
                    {
                        if (map != null)
                        {
                            if (biomeSpecificAnimals != null)
                            {
                                foreach (BiomeAnimalRecord animalRecord in biomeSpecificAnimals)
                                {
                                    PawnKindDef kindDef = animalRecord.animal;
                                    if (kindDef != null && kindDef.race.GetCompProperties<CompProperties_EggLayer>() != null)
                                    {
                                        eggLayingAnimals.Add(kindDef);
                                    }
                                }
                                Log.Message("[<color=#668cff>Random Chance</color>]" + "<color=#66ff8c> eggLayingAnimals: </color>" + string.Join(", ", eggLayingAnimals));
                            }

                            List<ThingDef> possibleEggs = new ();
                            for (int i = 0; i < eggLayingAnimals.Count; i++)
                            {
                                PawnKindDef kindDef = eggLayingAnimals[i];
                                Pawn pawn = PawnGenerator.GeneratePawn(kindDef, null); // Generate a temporary pawn instance for this PawnKindDef
                                CompEggLayer compEggLayer = pawn?.TryGetComp<CompEggLayer>(); // Try to get the CompEggLayer from the temporary pawn
                                if (compEggLayer != null)
                                {
                                    possibleEggs.Add(compEggLayer.Props?.eggUnfertilizedDef);
                                    possibleEggs.Add(compEggLayer.Props?.eggFertilizedDef);
                                }
                            }
                            Log.Message("[<color=#668cff>Random Chance</color>]" + "<color=#66ff8c> possibleEggs count: </color>" + string.Join(", ", possibleEggs));

                            if (possibleEggs.NullOrEmpty())
                                return;

                            ThingDef chosenEggDef = possibleEggs.RandomElement();
                            Log.Message("[<color=#668cff>Random Chance</color>]" + "<color=#66ff8c> chosenEggDef: </color>" + chosenEggDef.label);
                            if (chosenEggDef != null)
                            {
                                Thing eggs = ThingMaker.MakeThing(chosenEggDef, null);
                                eggs.stackCount = Rand.RangeInclusive(1, 3);
                                Log.Message("[<color=#668cff>Random Chance</color>]" + "<color=#66ff8c> eggs.stackCount: </color>" + eggs.stackCount);
                                GenPlace.TryPlaceThing(eggs, __instance.pawn.Position, map, ThingPlaceMode.Near);
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
