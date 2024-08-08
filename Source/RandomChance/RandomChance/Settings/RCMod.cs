using System;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class RCMod : Mod
    {
        public static RCMod mod;
        
        private RCSettings settings;
        private float halfWidth;
        private Vector2 leftScrollPos = Vector2.zero;
        private Vector2 rightScrollPos = Vector2.zero;
        private readonly Color _headerTextColor = new (1.0f, 0.4f, 0.4f);
        
        private const float _newSectionGap = 6f;
        private const float _headerTextGap = 3f;
        private const float _spacing = 10f;
        private const float _sliderSpacing = 120f;
        private const float _labelWidth = 200f;
        private const float _textFieldWidth = 100f;
        private const float _elementHeight = 25f;
        
        public RCMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<RCSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            halfWidth = (inRect.width - 30) / 2;
            LeftSideScrollViewHandler(new Rect(inRect.x, inRect.y, halfWidth, inRect.height));
            RightSideScrollViewHandler(new Rect(inRect.x + halfWidth + 20, inRect.y, halfWidth, inRect.height));
        }

        public override string SettingsCategory()
        {
            return "RC_ModName".Translate();
        }
        
        private void LeftSideScrollViewHandler(Rect inRect)
        {
            Listing_Standard list1 = new();
            list1.Gap(200f);
            Rect viewRect1 = new(inRect.x, inRect.y, inRect.width, inRect.height - 40);
            Rect vROffset1 = new(0, 0, inRect.width - 20, inRect.height - 100); // Adjust height as more settings are added
            
            Widgets.BeginScrollView(viewRect1, ref leftScrollPos, vROffset1);
            list1.Begin(vROffset1);
            list1.Gap(_newSectionGap);
            
            list1.CheckboxLabeled("RC_AllowMessages".Translate(), ref settings._allowMessages, "RC_AllowMessagesDesc".Translate());
            list1.Gap(_newSectionGap);

            list1.Label("RC_FilthyRoomConfigHeader".Translate().Colorize(_headerTextColor));
            list1.Gap(_headerTextGap);
            
            list1.CheckboxLabeled("RC_AllowFilthyRoomEvent".Translate(), ref settings._allowFilthyRoomEvent, "RC_AllowFilthyRoomEventDesc".Translate());
            list1.Gap(_spacing);
            
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomCooldownTicks".Translate(), 
                "RC_FilthyRoomCooldownTicksDesc".Translate(), ref settings._filthyRoomCooldownTicks, 
                1, 600000);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSampleInterval".Translate(), 
                "RC_FilthyRoomSampleIntervalDesc".Translate(), ref settings._filthyRoomSampleInterval, 
                1, 60000);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSampleChecks".Translate(), 
                "RC_FilthyRoomSampleChecksDesc".Translate(), ref settings._filthyRoomSampleChecks, 
                1, 10);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSpawnThreshold".Translate(), 
                "RC_FilthyRoomSpawnThresholdDesc".Translate(), ref settings._filthyRoomSpawnThreshold, 
                1, 25);
            DrawSettingWithSliderAndTextField(vROffset1, list1, "RC_FilthyRoomSpawnMHChance".Translate(), 
                "RC_FilthyRoomSpawnMHChanceDesc".Translate(), ref settings._filthyRoomSpawnMHChance, 
                0f, 1f);
            DrawSettingWithIntRangeAndText(vROffset1, list1, "RC_FilthyRoomPestSpawnRange".Translate(), 
                "RC_FilthyRoomPestSpawnRangeDesc".Translate(), ref settings._filthyRoomPestSpawnRange, 
                1, 10);
            list1.Gap(_newSectionGap);
            
            list1.End();
            Widgets.EndScrollView();
        }
        
        private void RightSideScrollViewHandler(Rect inRect)
        {
            Listing_Standard list2 = new ();
            Rect viewRect2 = new(inRect.x, inRect.y, inRect.width, inRect.height - 40);
            Rect vROffset2 = new(0, 0, inRect.width - 20, inRect.height + 375); // Adjust height as more settings are added
            
            Widgets.BeginScrollView(viewRect2, ref rightScrollPos, vROffset2);
            list2.Begin(vROffset2);
            list2.Gap(_newSectionGap);

            list2.Label("RC_CookingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CookingFailureChance".Translate(), 
                "RC_CookingFailureChanceDesc".Translate(), ref settings._cookingFailureChance, 
                0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_FailedCookingFireSize".Translate(), 
                "RC_FailedCookingFireSizeDesc".Translate(), ref settings._failedCookingFireSize, 
                0f, 15f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CookingBetterMealChance".Translate(), 
                "RC_CookingBetterMealChanceDesc".Translate(), ref settings._cookingBetterMealChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_ButcheringHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ButcheringFailureChance".Translate(), 
                "RC_ButcheringFailureChanceDesc".Translate(), ref settings._butcheringFailureChance, 
                0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ButcherMessRadius".Translate(), 
                "RC_ButcherMessRadiusDesc".Translate(), ref settings._butcherMessRadius, 
                1, 5);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_BonusButcherProductChance".Translate(), 
                "RC_BonusButcherProductChanceDesc".Translate(), ref settings._bonusButcherProductChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_CrematingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_CrematingInjuryChance".Translate(), 
                "RC_CrematingInjuryChanceDesc".Translate(), ref settings._crematingInjuryChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_RepairingHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairFailureChance".Translate(), 
                "RC_ElectricalRepairFailureChanceDesc".Translate(), ref settings._electricalRepairFailureChance, 
                0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairFireChance".Translate(), 
                "RC_ElectricalRepairFireChanceDesc".Translate(), ref settings._electricalRepairFireChance, 
                0f, 1f);
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_ElectricalRepairShortCircuitChance".Translate(), 
                "RC_ElectricalRepairShortCircuitChanceDesc".Translate(), ref settings._electricalRepairShortCircuitChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_PlantWorkHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_PlantHarvestingFindEggsChance".Translate(), 
                "RC_PlantHarvestingFindEggsChanceDesc".Translate(), ref settings._plantHarvestingFindEggsChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_AnimalWorkHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_HurtByFarmAnimalChance".Translate(), 
                "RC_HurtByFarmAnimalChanceDesc".Translate(), ref settings._hurtByFarmAnimalChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.Label("RC_MiscStuffHeader".Translate().Colorize(_headerTextColor));
            list2.Gap(_headerTextGap);
            
            DrawSettingWithSliderAndTextField(vROffset2, list2, "RC_InjuredByApparelChance".Translate(), 
                "RC_InjuredByApparelChanceDesc".Translate(), ref settings._injuredByApparelChance, 
                0f, 1f);
            list2.Gap(_newSectionGap);

            list2.End();
            Widgets.EndScrollView();
        }
        
        private static void DrawSettingWithSliderAndTextField<T>(Rect inRect, Listing_Standard list, string labelText, string tooltipText, ref T settingValue, T minValue, T maxValue) where T : struct, IConvertible
        {
            float sliderWidth = inRect.width - _sliderSpacing;

            // Convert settingValue, minValue, and maxValue to float for the slider
            float settingFloat = Convert.ToSingle(settingValue);
            float minFloat = Convert.ToSingle(minValue);
            float maxFloat = Convert.ToSingle(maxValue);

            // Draw the label with tooltip
            Rect labelRect = new(0, list.CurHeight, _labelWidth, _elementHeight);
            Widgets.Label(labelRect, labelText);
            TooltipHandler.TipRegion(labelRect, tooltipText);

            // Draw the text field aligned to the right of the main rect
            Rect textFieldRect = new(sliderWidth - _spacing, list.CurHeight, _textFieldWidth, _elementHeight);
            string textValue = typeof(T) == typeof(int) ? settingFloat.ToString("F0") : settingFloat.ToString("F2");
            Widgets.TextFieldNumeric(textFieldRect, ref settingFloat, ref textValue, minFloat, maxFloat);

            list.Gap(_spacing * 1.75f);

            // Draw the slider or the range selector
            if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                Rect sliderRect = new(0, list.CurHeight, sliderWidth + _textFieldWidth + _spacing, _elementHeight);
                float sliderValue = settingFloat;
                sliderValue = Widgets.HorizontalSlider(sliderRect, sliderValue, minFloat, maxFloat, true);
                settingFloat = sliderValue;

                // Update the settingValue after the slider adjustment
                settingValue = (T)Convert.ChangeType(settingFloat, typeof(T));
            }
            
            // Move the list's cursor down after the slider
            list.Gap(_spacing);
            list.Gap(30.00f);
        }
        
        private static void DrawSettingWithIntRangeAndText(Rect inRect, Listing_Standard list, string labelText, string tooltipText, ref IntRange settingValue, int minValue, int maxValue)
        {
            float rangeWidth = inRect.width - _sliderSpacing;
            const float textFieldWidth = (_textFieldWidth / 2) - _spacing;

            // Draw the label with tooltip
            Rect labelRect = new(0, list.CurHeight, _labelWidth, _elementHeight);
            Widgets.Label(labelRect, labelText);
            TooltipHandler.TipRegion(labelRect, tooltipText);

            // Draw the min and max text fields aligned to the right of the main rect
            Rect maxTextFieldRect = new(rangeWidth + textFieldWidth + _spacing, list.CurHeight, textFieldWidth, _elementHeight);
            Rect minTextFieldRect = new(maxTextFieldRect.x - maxTextFieldRect.width - _spacing, list.CurHeight, textFieldWidth, _elementHeight);
            
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
            
            list.Gap(_spacing);
            list.Gap(30.00f);
        }
    }
}
