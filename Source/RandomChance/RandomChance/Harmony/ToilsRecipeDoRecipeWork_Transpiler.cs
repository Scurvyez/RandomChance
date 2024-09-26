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
    public static class ToilsRecipeDoRecipeWork_Transpiler
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
            MethodBase addition = AccessTools.Method(typeof(ToilsRecipeDoRecipeWork_Transpiler), nameof(TryGiveRandomFailure));

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
            if (actor.IsColonyMech || RCDefOf.RC_ConfigCurves == null || actor.RaceProps.Animal) return;
            bool startFire = false;
            bool giveInjury = false;

            int pawnsAvgSkillLevel = (int)actor.skills.AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);
            building = curJob.GetTarget(TargetIndex.A).Thing as Building_WorkTable;

            if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (jobDriver.ticksSpentDoingRecipeWork == 1)
                {
                    if (Rand.Chance(RCSettings.CookingFailureChance) 
                        && Rand.Chance(RCDefOf.RC_ConfigCurves.cookingFailureCurve.Evaluate(pawnsAvgSkillLevel)))
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

                if (startFire)
                {
                    RCFailureUtil.StartFireHandler(actor, curJob, building);
                }
                if (giveInjury)
                {
                    RCFailureUtil.GiveInjuryHandler(actor, curJob, building);
                }
            }

            else if (curJob.RecipeDef == RCDefOf.ButcherCorpseFlesh)
            {
                if (jobDriver.ticksSpentDoingRecipeWork != 1) return;
                if (!Rand.Chance(RCSettings.ButcheringFailureChance)) return;
                if (Rand.Chance(RCDefOf.RC_ConfigCurves.butcheringMessCurve.Evaluate(pawnsAvgSkillLevel)))
                {
                    RCFailureUtil.CauseMessHandler(actor, curJob, building);
                }
            }

            else if (curJob.RecipeDef == RCDefOf.CremateCorpse)
            {
                if (jobDriver.ticksSpentDoingRecipeWork != 1) return;
                if (!Rand.Chance(RCSettings.CrematingInjuryChance)) return;
                if (Rand.Chance(RCDefOf.RC_ConfigCurves.crematingInjuryCurve.Evaluate(pawnsAvgSkillLevel)))
                {
                    RCFailureUtil.GiveInjuryHandler(actor, curJob, building);
                }
            }
        }
    }
}
