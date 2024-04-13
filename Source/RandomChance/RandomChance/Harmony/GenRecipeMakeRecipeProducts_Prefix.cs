using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RandomChance
{
    /*
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GenRecipeMakeRecipeProducts_Prefix
    {
        private static readonly Dictionary<RecipeDef, RecipeDef> recipeMap = new ()
        {
            { RandomChance_DefOf.CookMealSimple, RandomChance_DefOf.CookMealFine },
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
            if (!worker.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
            {
                int pawnsAvgSkillLevel = (int)worker.skills.AverageOfRelevantSkillsFor(billGiver.GetWorkgiver().workType);
                float betterQualityMealChance = RandomChanceSettings.CookingBetterMealChance; // 5%
                SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.betterMealOutcomeCurve;

                if (Rand.Chance(betterQualityMealChance))
                {
                    if (recipeMap.TryGetValue(recipeDef, out RecipeDef newRecipeDef))
                    {
                        if (Rand.Chance(injuryCurve.Evaluate(pawnsAvgSkillLevel)))
                        {
                            recipeDef = newRecipeDef;

                            if (RandomChanceSettings.AllowMessages)
                            {
                                Messages.Message("RC_BetterMealProduced".Translate(worker.Named("PAWN")),
                                MessageTypeDefOf.PositiveEvent);
                            }
                        }
                    }
                }
            }
        }
    }
    */
}
