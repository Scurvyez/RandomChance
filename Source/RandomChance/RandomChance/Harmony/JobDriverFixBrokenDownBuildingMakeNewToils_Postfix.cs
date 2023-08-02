using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch(typeof(JobDriver_FixBrokenDownBuilding), "MakeNewToils")]
    public class JobDriverFixBrokenDownBuildingMakeNewToils_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_FixBrokenDownBuilding __instance)
        {
            List<Toil> newToils = new (__result);
            int numToils = newToils.Count;

            for (int i = 0; i < numToils; i++)
            {
                Toil toil = newToils[i];

                if (i == numToils - 1) // Check if this is the last toil
                {
                    toil.initAction = delegate
                    {
                        if (!__instance.pawn.IsColonyMech)
                        {
                            float failureChance = 0.85f; // 5% by default
                            if (Rand.Chance(failureChance))
                            {
                                IBillGiverWithTickAction billGiver = __instance.job.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction;
                                int pawnsAvgSkillLevel = (int)__instance.pawn.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
                                Building building = __instance.job.GetTarget(TargetIndex.A).Thing as Building;

                                SimpleCurve chanceCurve = new()
                                {
                                    { 0, 0.5f },
                                    { 3, 0.5f },
                                    { 6, 0.3f },
                                    { 8, 0.2f },
                                    { 14, 0.1f },
                                    { 18, 0.05f },
                                    { 20, 0.02f }
                                };

                                if (Rand.Chance(0.85f)) // change back to evaluate the curve
                                {
                                    if (building != null)
                                    {
                                        IntVec3 explosionCenter = building.Position;
                                        Map explosionMap = building.Map;
                                        int explosionRadius = Rand.RangeInclusive(2, 5); // Adjust the explosion radius as needed

                                        // make some filth too, go all out
                                        GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.Bomb, null, Rand.RangeInclusive(3, 15));
                                        GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.EMP, null, Rand.RangeInclusive(1, 12));
                                    }

                                    BodyPartRecord bodyPart = __instance.pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbCore).RandomElement();

                                    if (bodyPart != null)
                                    {
                                        Thing Components = __instance.job.GetTarget(TargetIndex.B).Thing;
                                        Components.Destroy();

                                        HediffDef burnHediffDef = HediffDefOf.Burn;

                                        float severity = 0.9f; // change later

                                        Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, __instance.pawn, bodyPart);
                                        hediff.Severity = severity;
                                        __instance.pawn.health.AddHediff(hediff);

                                        Find.TickManager.slower.SignalForceNormalSpeedShort();
                                        __instance.pawn.stances.stunner.StunFor(120, __instance.pawn, false, false);
                                    }
                                }
                            }
                        }
                    };
                }
                newToils[i] = toil;
            }
            __result = newToils;
        }
    }
}
