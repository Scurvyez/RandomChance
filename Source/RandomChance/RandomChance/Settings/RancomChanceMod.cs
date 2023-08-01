using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class RancomChanceMod : Mod
    {
        RandomChanceSettings settings;
        public static RancomChanceMod mod;

        public RancomChanceMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<RandomChanceSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            Listing_Standard list = new();
            Rect viewRect = new(inRect.x, inRect.y, inRect.width, inRect.height);
            list.Begin(viewRect);

            // Cooking
            list.Label("<color=#ff6666>Cooking</color>");
            list.Gap(3.00f);

            float cookingFailureChanceSlider = settings._cookingFailureChance;
            string cookingFailureChanceSliderText = cookingFailureChanceSlider.ToString("F2");
            list.Label(label: "RC_CookingFailureChance".Translate(cookingFailureChanceSliderText), tooltip: "RC_CookingFailureChanceDesc".Translate());
            settings._cookingFailureChance = list.Slider(settings._cookingFailureChance, 0.0f, 1.0f);

            float failedCookingFireSizeSlider = settings._failedCookingFireSize;
            string failedCookingFireSizeSliderText = failedCookingFireSizeSlider.ToString("F2");
            list.Label(label: "RC_FailedCookingFireSize".Translate(failedCookingFireSizeSliderText), tooltip: "RC_FailedCookingFireSizeDesc".Translate());
            settings._failedCookingFireSize = list.Slider(settings._failedCookingFireSize, 1.0f, 15.0f);

            float cookingBetterMealChanceSlider = settings._cookingBetterMealChance;
            string cookingBetterMealChanceSliderText = cookingBetterMealChanceSlider.ToString("F2");
            list.Label(label: "RC_CookingBetterMealChance".Translate(cookingBetterMealChanceSliderText), tooltip: "RC_CookingBetterMealChanceDesc".Translate());
            settings._cookingBetterMealChance = list.Slider(settings._cookingBetterMealChance, 0.0f, 1.0f);

            // Butchering
            list.Label("<color=#ff6666>Butchering</color>");
            list.Gap(3.00f);

            float butcheringFailureChanceSlider = settings._butcheringFailureChance;
            string butcheringFailureChanceSliderText = butcheringFailureChanceSlider.ToString("F2");
            list.Label(label: "RC_ButcheringFailureChance".Translate(butcheringFailureChanceSliderText), tooltip: "RC_ButcheringFailureChanceDesc".Translate());
            settings._butcheringFailureChance = list.Slider(settings._butcheringFailureChance, 0.0f, 1.0f);

            float butcherMessRadiusSlider = settings._butcherMessRadius;
            string butcherMessRadiusSliderText = butcherMessRadiusSlider.ToString("F0");
            list.Label(label: "RC_ButcherMessRadius".Translate(butcherMessRadiusSliderText), tooltip: "RC_ButcherMessRadiusDesc".Translate());
            settings._butcherMessRadius = (int)list.Slider(settings._butcherMessRadius, 1, 5);

            float bonusButcherProductChanceSlider = settings._bonusButcherProductChance;
            string bonusButcherProductChanceSliderText = bonusButcherProductChanceSlider.ToString("F2");
            list.Label(label: "RC_BonusButcherProductChance".Translate(bonusButcherProductChanceSliderText), tooltip: "RC_BonusButcherProductChanceDesc".Translate());
            settings._bonusButcherProductChance = list.Slider(settings._bonusButcherProductChance, 0.0f, 1.0f);

            // Cremating
            list.Label("<color=#ff6666>Cremating</color>");
            list.Gap(3.00f);

            float crematingInjuryChanceSlider = settings._crematingInjuryChance;
            string crematingInjuryChanceSliderText = crematingInjuryChanceSlider.ToString("F2");
            list.Label(label: "RC_CrematingInjuryChance".Translate(crematingInjuryChanceSliderText), tooltip: "RC_CrematingInjuryChanceDesc".Translate());
            settings._crematingInjuryChance = list.Slider(settings._crematingInjuryChance, 0.0f, 1.0f);

            list.End();
        }

        public override string SettingsCategory()
        {
            return "RC_ModName".Translate();
        }
    }
}
