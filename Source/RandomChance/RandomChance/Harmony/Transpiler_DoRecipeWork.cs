using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch]
    public static class DoRecipeWork_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo targetMethod = typeof(Toils_Recipe).GetNestedTypes(AccessTools.all).SelectMany(innerType => AccessTools.GetDeclaredMethods(innerType))
                    .FirstOrDefault(method => method.Name.Contains("<DoRecipeWork>b__1") && method.ReturnType == typeof(void));
            yield return targetMethod;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo target = AccessTools.Method(typeof(IBillGiverWithTickAction), "UsedThisTick");
            MethodBase addition = AccessTools.Method(typeof(DoRecipeWork_Patch), nameof(TryGiveRandomFailure));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;

                if (instruction.Calls(target))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0); // Load 'actor' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_1); // Load 'curJob' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // Load 'jobDriver' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4); // Load 'billGiver' onto the stack
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 7); // Load 'building' onto the stack
                    yield return new CodeInstruction(OpCodes.Call, addition); // Call the TryGiveRandomFailure method
                    continue;
                }
            }
        }

        public static void TryGiveRandomFailure(Pawn actor, Job curJob, JobDriver_DoBill jobDriver, IBillGiverWithTickAction billGiver, Building_WorkTable building)
        {
            bool startFire = false;
            bool giveInjury = false;

            float failureChance = 0.05f; // 5%
            float messChance = 0.5f; // 9%
            int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);

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

            if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (jobDriver.ticksSpentDoingRecipeWork == 1)
                {
                    if (Rand.Chance(failureChance))
                    {
                        if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
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
                    GiveInjuryHandler(actor, billGiver, building);
                }
            }

            if (curJob.RecipeDef == RandomChance_DefOf.ButcherCorpseFlesh)
            {
                if (jobDriver.ticksSpentDoingRecipeWork == 1)
                {
                    if (Rand.Chance(messChance))
                    {
                        if (Rand.Chance(chanceCurve.Evaluate(pawnsAvgSkillLevel)))
                        {
                            IntVec3 buildingPos = building.Position;
                            Map map = building.Map;
                            IntVec3 adjacentCell = buildingPos + GenAdj.CardinalDirections.RandomElement();
                            FilthMaker.TryMakeFilth(adjacentCell, map, ThingDefOf.Filth_Blood);
                        }
                    }
                }
            }
        }

        private static void StartFireHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            Messages.Message("RC_FireInKitchen".Translate(actor.Named("PAWN")), actor, MessageTypeDefOf.NegativeEvent);
            Thing ingredients = curJob.GetTarget(TargetIndex.B).Thing;
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;

            if (ingredients != null)
            {
                ingredients.Destroy();

                FireUtility.TryStartFireIn(buildingPos, map, 7.5f);
                actor.stances.stunner.StunFor(240, actor, false, false);
            }
        }

        private static void GiveInjuryHandler(Pawn actor, IBillGiverWithTickAction billGiver, Building_WorkTable building)
        {
            Messages.Message("RC_InjuryInKitchen".Translate(actor.Named("PAWN")), actor, MessageTypeDefOf.NegativeEvent);

            IntVec3 buildingPos = building.Position;
            Map map = building.Map;
            HediffDef burnHediffDef = HediffDefOf.Burn;
            HediffDef cutHediffDef = HediffDefOf.Cut;
            int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
            
            float severity = pawnsAvgSkillLevel switch
            {
                < 5 => 0.1f,
                <= 10 => 0.3f,
                <= 15 => 0.5f,
                <= 18 => 0.7f,
                <= 20 => 0.9f,
                _ => 1.0f,
            };

            BodyPartRecord fingersPart = actor.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit).RandomElement();

            if (Rand.Bool)
            {
                Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, fingersPart);
                hediff.Severity = severity;
                actor.health.AddHediff(hediff);
            }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(cutHediffDef, actor, fingersPart);
                hediff.Severity = severity;
                actor.health.AddHediff(hediff);

                IntVec3 adjacentCell = buildingPos + GenAdj.CardinalDirections.RandomElement();
                FilthMaker.TryMakeFilth(adjacentCell, map, ThingDefOf.Filth_Blood);
            }
        }
    }
}
