using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
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
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Log.Message("[RC] hi");

            // new toil with delegate
            Toil customToil = new Toil
            {
                tickAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech)
                    {
                        float failureChance = RandomChanceSettings.ElectricalRepairFailureChance; // 5% by default
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

                            if (Rand.Chance(0.5f)) // change back to evaluate the curve
                            {
                                if (building != null)
                                {
                                    IntVec3 explosionCenter = building.Position;
                                    Map explosionMap = building.Map;
                                    int explosionRadius = Rand.RangeInclusive(2, 5);

                                    // make some filth too, go all out
                                    GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.EMP, null, Rand.RangeInclusive(1, 9));
                                    if (Rand.Value < 0.25f)
                                    {
                                        GenExplosion.DoExplosion(explosionCenter, explosionMap, explosionRadius, DamageDefOf.Bomb, null, Rand.RangeInclusive(3, 12));
                                    }
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
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
    }
}
