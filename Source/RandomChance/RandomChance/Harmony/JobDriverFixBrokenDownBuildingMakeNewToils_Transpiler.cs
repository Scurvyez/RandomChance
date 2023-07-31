using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RandomChance
{
    /*
    [HarmonyPatch]
    public static class JobDriverFixBrokenDownBuildingMakeNewToils_Transpiler
    {
        private static readonly CodeMatch[] toMatch = new CodeMatch[]
        {
            new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(StatDefOf), nameof(StatDefOf.ConstructSuccessChance))),
            new CodeMatch(OpCodes.Ldc_I4_1),
            new CodeMatch(OpCodes.Ldc_I4_M1),
            new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue))),
            new CodeMatch(OpCodes.Stloc_S)
        };

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> CalculateMethods(Harmony instance)
        {
            var candidates = typeof(JobDriver_FixBrokenDownBuilding).GetNestedTypes(AccessTools.all).SelectMany(t => AccessTools.GetDeclaredMethods(t));

            foreach (var method in candidates)
            {
                var instructions = PatchProcessor.GetCurrentInstructions(method);
                var matched = new CodeMatcher(instructions).MatchStartForward(toMatch).IsValid;
                if (matched)
                    yield return method;
            }
            yield break;
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MakeNewToils_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions);

            CodeInstruction[] toInsert = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobDriverFixBrokenDownBuildingMakeNewToils_Transpiler), nameof(TryElectrocutePawnAndFailJob)))
            };

            codeMatcher.MatchEndForward(toMatch);
            codeMatcher.Insert(toInsert);
            codeMatcher.End();

            if (codeMatcher.IsInvalid)
            {
                Log.Warning("[RC] Failed to apply transpiler on JobDriverFixBrokenDownBuildingMakeNewToils_Transpiler!");
                return instructions;
            }
            else
                return codeMatcher.InstructionEnumeration();
        }

        public static void TryElectrocutePawnAndFailJob(Pawn pawn)
        {
            Log.Warning("[RC] Patch is running.");
        }
    }
    */
}
