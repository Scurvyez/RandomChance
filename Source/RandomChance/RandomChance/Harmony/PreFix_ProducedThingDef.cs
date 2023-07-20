﻿using System.Collections.Generic;

using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomChance
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class AlternateRecipe_Patch
    {
        private static readonly Dictionary<RecipeDef, RecipeDef> recipeMap = new ()
        {
            { RecipeDefOf.CookMealSimple, RandomChance_DefOf.CookMealFine },
            { RandomChance_DefOf.CookMealFine, RandomChance_DefOf.CookMealLavish },
            { RandomChance_DefOf.CookMealSimpleBulk, RandomChance_DefOf.CookMealFineBulk },
            { RandomChance_DefOf.CookMealFine_Veg, RandomChance_DefOf.CookMealLavish_Veg },
            { RandomChance_DefOf.CookMealFine_Meat, RandomChance_DefOf.CookMealLavish_Meat },
            { RandomChance_DefOf.CookMealFineBulk, RandomChance_DefOf.CookMealLavishBulk },
            { RandomChance_DefOf.CookMealFineBulk_Veg, RandomChance_DefOf.CookMealLavishBulk_Veg },
            { RandomChance_DefOf.CookMealFineBulk_Meat, RandomChance_DefOf.CookMealLavishBulk_Meat }
        };

        [HarmonyPrefix]
        public static void Prefix(ref RecipeDef recipeDef, Pawn worker, IBillGiver billGiver)
        {
            int pawnsAvgSkillLevel = (int)worker.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
            float betterQualityMealChance = 0.05f; // 5%

            if (Rand.Chance(betterQualityMealChance))
            {
                if (recipeMap.TryGetValue(recipeDef, out RecipeDef newRecipeDef))
                {
                    SimpleCurve higherQualityCurve = new()
                    {
                        { 0, 0 },
                        { 3, 0 },
                        { 6, 0.02f },
                        { 8, 0.04f },
                        { 14, 0.09f },
                        { 18, 0.99f },
                        { 20, 0.99f }
                    };

                    if (Rand.Chance(higherQualityCurve.Evaluate(pawnsAvgSkillLevel)))
                    {
                        recipeDef = newRecipeDef;
                        Messages.Message("RC_BetterMealProduced".Translate(worker.Named("PAWN")), MessageTypeDefOf.PositiveEvent);
                    }
                }
            }
        }
    }
}