using System;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class RCMod : Mod
    {
        public static RCMod mod;
        
        private readonly RCSettings _settings;
        private float _halfWidth;
        private Vector2 _leftScrollPos = Vector2.zero;
        private Vector2 _rightScrollPos = Vector2.zero;
        private readonly Color _headerTextColor = new (1.0f, 0.4f, 0.4f);
        
        private const float NewSectionGap = 6f;
        private const float HeaderTextGap = 3f;
        private const float Spacing = 10f;
        private const float SliderSpacing = 120f;
        private const float LabelWidth = 200f;
        private const float TextFieldWidth = 100f;
        private const float ElementHeight = 25f;
        
        public RCMod(ModContentPack content) : base(content)
        {
            mod = this;
            _settings = GetSettings<RCSettings>();
        }
        
        public override string SettingsCategory()
        {
            return "RC_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            _halfWidth = (inRect.width - 30) / 2;
            LeftSideScrollViewHandler(new Rect(inRect.x, inRect.y, _halfWidth, inRect.height));
            RightSideScrollViewHandler(new Rect(inRect.x + _halfWidth + 20, inRect.y, _halfWidth, inRect.height));
        }
        
        private void LeftSideScrollViewHandler(Rect inRect)
        {
            Listing_Standard list1 = new();
            list1.Gap(200f);
            Rect viewRect1 = new(inRect.x, inRect.y, inRect.width, inRect.height - 40);
            Rect vROffset1 = new(0, 0, inRect.width - 20, inRect.height - 100); // Adjust height as more settings are added
            
            Widgets.BeginScrollView(viewRect1, ref _leftScrollPos, vROffset1);
            list1.Begin(vROffset1);
            list1.Gap(NewSectionGap);
            
            list1.CheckboxLabeled("RC_AllowMessages".Translate(), ref _settings._allowMessages,
                "RC_AllowMessagesDesc".Translate());
            list1.Gap(NewSectionGap);

            list1.Label("RC_FilthyRoomConfigHeader".Translate().Colorize(_headerTextColor));
            list1.Gap(HeaderTextGap);
            
            list1.CheckboxLabeled("RC_AllowFilthyRoomEvent".Translate(), ref _settings._allowFilthyRoomEvent, 
                "RC_AllowFilthyRoomEventDesc".Translate());
            list1.Gap(Spacing);
            
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomCooldownTicks".Translate(), 
                "RC_FilthyRoomCooldownTicksDesc".Translate(),
                ref _settings._filthyRoomCooldownTicks, 1, 600000);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSampleInterval".Translate(), 
                "RC_FilthyRoomSampleIntervalDesc".Translate(), 
                ref _settings._filthyRoomSampleInterval, 1, 60000);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSampleChecks".Translate(), 
                "RC_FilthyRoomSampleChecksDesc".Translate(),
                ref _settings._filthyRoomSampleChecks, 1, 10);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSpawnThreshold".Translate(), 
                "RC_FilthyRoomSpawnThresholdDesc".Translate(),
                ref _settings._filthyRoomSpawnThreshold, 1, 25);
            DrawSettingWithIntRangeAndText(vROffset1, list1, "RC_FilthyRoomPestSpawnRange".Translate(), 
                "RC_FilthyRoomPestSpawnRangeDesc".Translate(),
                ref _settings._filthyRoomPestSpawnRange, 1, 10);
            list1.Gap(NewSectionGap);
            
            list1.End();
            Widgets.EndScrollView();
        }
        
        private void RightSideScrollViewHandler(Rect inRect)
        {
            Listing_Standard list2 = new ();
            Rect viewRect2 = new(inRect.x, inRect.y, inRect.width, inRect.height - 40);
            Rect vROffset2 = new(0, 0, inRect.width - 20, inRect.height + 475); // Adjust height as more settings are added
            
            Widgets.BeginScrollView(viewRect2, ref _rightScrollPos, vROffset2);
            list2.Begin(vROffset2);
            list2.Gap(NewSectionGap);

            list2.Label("RC_CookingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CookingFailureChance".Translate(), 
                "RC_CookingFailureChanceDesc".Translate(),
                ref _settings._cookingFailureChance, 0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_FailedCookingFireSize".Translate(), 
                "RC_FailedCookingFireSizeDesc".Translate(),
                ref _settings._failedCookingFireSize, 0f, 15f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CookingBetterMealChance".Translate(), 
                "RC_CookingBetterMealChanceDesc".Translate(),
                ref _settings._cookingBetterMealChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_ButcheringHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ButcheringFailureChance".Translate(), 
                "RC_ButcheringFailureChanceDesc".Translate(), 
                ref _settings._butcheringFailureChance, 0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ButcherMessRadius".Translate(), 
                "RC_ButcherMessRadiusDesc".Translate(), 
                ref _settings._butcherMessRadius, 1, 5);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_BonusButcherProductChance".Translate(), 
                "RC_BonusButcherProductChanceDesc".Translate(),
                ref _settings._bonusButcherProductChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_CrematingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CrematingInjuryChance".Translate(), 
                "RC_CrematingInjuryChanceDesc".Translate(),
                ref _settings._crematingInjuryChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_RepairingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairFailureChance".Translate(), 
                "RC_ElectricalRepairFailureChanceDesc".Translate(), 
                ref _settings._electricalRepairFailureChance, 0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairFireChance".Translate(), 
                "RC_ElectricalRepairFireChanceDesc".Translate(), 
                ref _settings._electricalRepairFireChance, 0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairShortCircuitChance".Translate(), 
                "RC_ElectricalRepairShortCircuitChanceDesc".Translate(),
                ref _settings._electricalRepairShortCircuitChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_PlantWorkHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_PlantHarvestingFindEggsChance".Translate(), 
                "RC_PlantHarvestingFindEggsChanceDesc".Translate(),
                ref _settings._plantHarvestingFindEggsChance, 0f, 1f);
            list2.Gap(NewSectionGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_PlantHarvestAgitatedWildAnimalChance".Translate(), 
                "RC_PlantHarvestAgitatedWildAnimalChanceDesc".Translate(),
                ref _settings._plantHarvestAgitatedWildAnimalChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_AnimalWorkHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_HurtByFarmAnimalChance".Translate(), 
                "RC_HurtByFarmAnimalChanceDesc".Translate(),
                ref _settings._hurtByFarmAnimalChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.Label("RC_MiscStuffHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(HeaderTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_InjuredByApparelChance".Translate(), 
                "RC_InjuredByApparelChanceDesc".Translate(),
                ref _settings._injuredByApparelChance, 0f, 1f);
            list2.Gap(NewSectionGap);

            list2.End();
            Widgets.EndScrollView();
        }
        
        private static void DrawSettingWithSliderAndTextField<T>(Rect inRect, Listing_Standard list, string labelText, 
            string tooltipText, ref T settingValue, T minValue, T maxValue) where T : struct, IConvertible
        {
            float sliderWidth = inRect.width - SliderSpacing;

            // Convert settingValue, minValue, and maxValue to float for the slider
            float settingFloat = Convert.ToSingle(settingValue);
            float minFloat = Convert.ToSingle(minValue);
            float maxFloat = Convert.ToSingle(maxValue);

            // Draw the label with a tooltip
            Rect labelRect = new(0, list.CurHeight, LabelWidth, ElementHeight);
            Widgets.Label(labelRect, labelText);
            TooltipHandler.TipRegion(labelRect, tooltipText);

            // Draw the text field aligned to the right of the main rect
            Rect textFieldRect = new(sliderWidth - Spacing, list.CurHeight, TextFieldWidth, ElementHeight);
            string textValue = typeof(T) == typeof(int) 
                ? settingFloat.ToString("F0") 
                : settingFloat.ToString("F2");
            Widgets.TextFieldNumeric(textFieldRect, ref settingFloat, ref textValue, minFloat, maxFloat);

            list.Gap(Spacing * 1.75f);

            // Draw the slider or the range selector
            if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                Rect sliderRect = new(0, list.CurHeight, sliderWidth + TextFieldWidth + Spacing, ElementHeight);
                float sliderValue = settingFloat;
                sliderValue = Widgets.HorizontalSlider(sliderRect, sliderValue, minFloat, maxFloat, true);
                settingFloat = sliderValue;

                // Update the settingValue after the slider adjustment
                settingValue = (T)Convert.ChangeType(settingFloat, typeof(T));
            }
            
            // Move the list's cursor down after the slider
            list.Gap(Spacing);
            list.Gap(30.00f);
        }
        
        private static void DrawSettingWithIntRangeAndText(Rect inRect, Listing_Standard list, string labelText,
            string tooltipText, ref IntRange settingValue, int minValue, int maxValue)
        {
            float rangeWidth = inRect.width - SliderSpacing;
            const float textFieldWidth = (TextFieldWidth / 2) - Spacing;

            // Draw the label with a tooltip
            Rect labelRect = new(0, list.CurHeight, LabelWidth, ElementHeight);
            Widgets.Label(labelRect, labelText);
            TooltipHandler.TipRegion(labelRect, tooltipText);

            // Draw the min and max text fields aligned to the right of the main rect
            Rect maxTextFieldRect = new(rangeWidth + textFieldWidth + Spacing, 
                list.CurHeight, textFieldWidth, ElementHeight);
            Rect minTextFieldRect = new(maxTextFieldRect.x - maxTextFieldRect.width - Spacing, 
                list.CurHeight, textFieldWidth, ElementHeight);
            
            string minText = settingValue.min.ToString();
            string maxText = settingValue.max.ToString();

            Widgets.TextFieldNumeric(minTextFieldRect, ref settingValue.min, ref minText, minValue, maxValue);
            Widgets.TextFieldNumeric(maxTextFieldRect, ref settingValue.max, ref maxText, minValue, maxValue);

            // Ensure min value does not exceed max value
            if (settingValue.min > settingValue.max)
            {
                settingValue.min = settingValue.max;
            }
            // Ensure max value does not go below min value
            if (settingValue.max < settingValue.min)
            {
                settingValue.max = settingValue.min;
            }
            
            list.Gap(Spacing);
            list.Gap(30.00f);
        }
    }
}
