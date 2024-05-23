using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RandomChance
{
    public static class RCFoodUtil
    {
        public static readonly Dictionary<RecipeDef, RecipeDef> RecipeMap = new()
        {
            { RCDefOf.CookMealSimple, RCDefOf.CookMealFine },
            { RCDefOf.CookMealFine, RCDefOf.CookMealLavish },
            { RCDefOf.CookMealSimpleBulk, RCDefOf.CookMealFineBulk },
            { RCDefOf.CookMealFine_Veg, RCDefOf.CookMealLavish_Veg },
            { RCDefOf.CookMealFine_Meat, RCDefOf.CookMealLavish_Meat },
            { RCDefOf.CookMealFineBulk, RCDefOf.CookMealLavishBulk },
            { RCDefOf.CookMealFineBulk_Veg, RCDefOf.CookMealLavishBulk_Veg },
            { RCDefOf.CookMealFineBulk_Meat, RCDefOf.CookMealLavishBulk_Meat }
        };
        
        public static ThingDef GetRandomMeatFromOtherPawn()
        {
            List<ThingDef> meatDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(def => def.IsWithinCategory(ThingCategoryDefOf.MeatRaw))
                .ToList();

            return meatDefs.Count > 0 ? meatDefs.RandomElement() : ThingDefOf.Meat_Human;
        }
    }
}
