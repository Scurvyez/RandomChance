using Verse;
using RimWorld;

namespace RandomChance
{
    [DefOf]
    public class RCDefOf
    {
        public static ThingDef ElectricStove;
        public static ThingDef FueledStove;
        public static ThingDef MealLavish;
        public static ThingDef Apparel_PowerArmor;
        public static ThingDef Apparel_PowerArmorHelmet;

        public static PawnKindDef Rat;

        public static FleckDef RC_ElectricShock;

        public static HediffDef Burn;
        public static HediffDef RC_ElectricShockHediff;

        public static IncidentDef ShortCircuit;
        public static IncidentDef RC_FlickeringLights;

        public static ShaderTypeDef TransparentPostLight;

        public static RecipeDef CookMealSimple;
        public static RecipeDef CookMealSimpleBulk;
        public static RecipeDef CookMealFine;
        public static RecipeDef CookMealFine_Veg;
        public static RecipeDef CookMealFine_Meat;
        public static RecipeDef CookMealFineBulk;
        public static RecipeDef CookMealFineBulk_Meat;
        public static RecipeDef CookMealFineBulk_Veg;
        public static RecipeDef CookMealLavish;
        public static RecipeDef CookMealLavish_Meat;
        public static RecipeDef CookMealLavish_Veg;
        public static RecipeDef CookMealLavishBulk;
        public static RecipeDef CookMealLavishBulk_Veg;
        public static RecipeDef CookMealLavishBulk_Meat;
        public static RecipeDef ButcherCorpseFlesh;
        public static RecipeDef CremateCorpse;

        public static ConfigCurvesDef RC_ConfigCurves;
        public static ConfigMiscDef RC_ConfigMisc;

        static RCDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(RCDefOf));
        }
    }
}
