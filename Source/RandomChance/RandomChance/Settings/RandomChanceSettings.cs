using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RandomChance
{
    public class RandomChanceSettings : ModSettings
    {
        private static RandomChanceSettings _instance;

        public static float CookingFailureChance
        {
            get
            {
                return _instance._cookingFailureChance;
            }
        }

        public static float FailedCookingFireSize
        {
            get
            {
                return _instance._failedCookingFireSize;
            }
        }

        public static float CookingBetterMealChance
        {
            get
            {
                return _instance._cookingBetterMealChance;
            }
        }

        public static float ButcheringFailureChance
        {
            get
            {
                return _instance._butcheringFailureChance;
            }
        }

        public static float CrematingInjuryChance
        {
            get
            {
                return _instance._crematingInjuryChance;
            }
        }

        public float _cookingFailureChance = 0.05f;
        public float _failedCookingFireSize = 7.5f;
        public float _cookingBetterMealChance = 0.05f;
        public float _butcheringFailureChance = 0.09f;
        public float _crematingInjuryChance = 0.05f;

        public RandomChanceSettings()
        {
            _instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _cookingFailureChance, "cookingFailureChance", 0.05f);
            Scribe_Values.Look(ref _failedCookingFireSize, "failedCookingFireSize", 7.5f);
            Scribe_Values.Look(ref _cookingBetterMealChance, "cookingBetterMealChance", 0.05f);
            Scribe_Values.Look(ref _butcheringFailureChance, "butcheringFailureChance", 0.09f);
            Scribe_Values.Look(ref _crematingInjuryChance, "crematingInjuryChance", 0.05f);
        }
    }
}
