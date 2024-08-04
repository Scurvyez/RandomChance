using Verse;

namespace RandomChance
{
    public class RCSettings : ModSettings
    {
        private static RCSettings _instance;
        
        public RCSettings()
        {
            _instance = this;
        }

        public static float CookingFailureChance => _instance._cookingFailureChance;
        public static float FailedCookingFireSize => _instance._failedCookingFireSize;
        public static float CookingBetterMealChance => _instance._cookingBetterMealChance;
        public static float ButcheringFailureChance => _instance._butcheringFailureChance;
        public static int ButcherMessRadius => _instance._butcherMessRadius;
        public static float BonusButcherProductChance => _instance._bonusButcherProductChance;
        public static float CrematingInjuryChance => _instance._crematingInjuryChance;
        public static float ElectricalRepairFailureChance => _instance._electricalRepairFailureChance;
        public static float ElectricalRepairFireChance => _instance._electricalRepairFireChance;
        public static float ElectricalRepairShortCircuitChance => _instance._electricalRepairShortCircuitChance;
        public static float PlantHarvestingFindEggsChance => _instance._plantHarvestingFindEggsChance;
        public static float HurtByFarmAnimalChance => _instance._hurtByFarmAnimalChance;
        public static float InjuredByApparelChance => _instance._injuredByApparelChance;
        public static bool AllowMessages => _instance._allowMessages;
        public static bool AllowFilthyRoomEvent => _instance._allowFilthyRoomEvent;
        public static int FilthyRoomCooldownTicks => _instance._filthyRoomCooldownTicks;
        public static int FilthyRoomSampleInterval => _instance._filthyRoomSampleInterval;
        public static int FilthyRoomSampleChecks => _instance._filthyRoomSampleChecks;
        public static int FilthyRoomSpawnThreshold => _instance._filthyRoomSpawnThreshold;
        public static float FilthyRoomSpawnMHChance => _instance._filthyRoomSpawnMHChance;
        public static IntRange FilthyRoomPestSpawnRange => _instance._filthyRoomPestSpawnRange;
        
        public float _cookingFailureChance = 0.05f;
        public float _failedCookingFireSize = 7.5f;
        public float _cookingBetterMealChance = 0.05f;
        public float _butcheringFailureChance = 0.09f;
        public int _butcherMessRadius = 2;
        public float _bonusButcherProductChance = 0.08f;
        public float _crematingInjuryChance = 0.05f;
        public float _electricalRepairFailureChance = 0.05f;
        public float _electricalRepairFireChance = 0.05f;
        public float _electricalRepairShortCircuitChance = 0.05f;
        public float _plantHarvestingFindEggsChance = 0.05f;
        public float _hurtByFarmAnimalChance = 0.05f;
        public float _injuredByApparelChance = 0.05f;
        public bool _allowMessages = true;
        public bool _allowFilthyRoomEvent = true;
        public int _filthyRoomCooldownTicks = 420000;
        public int _filthyRoomSampleInterval = 45000;
        public int _filthyRoomSampleChecks = 2;
        public int _filthyRoomSpawnThreshold = 12;
        public float _filthyRoomSpawnMHChance = 0.2f;
        public IntRange _filthyRoomPestSpawnRange = new (1, 10);
                
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _cookingFailureChance, "cookingFailureChance", 0.05f);
            Scribe_Values.Look(ref _failedCookingFireSize, "failedCookingFireSize", 7.5f);
            Scribe_Values.Look(ref _cookingBetterMealChance, "cookingBetterMealChance", 0.05f);
            Scribe_Values.Look(ref _butcheringFailureChance, "butcheringFailureChance", 0.09f);
            Scribe_Values.Look(ref _butcherMessRadius, "butcherMessRadius", 2);
            Scribe_Values.Look(ref _bonusButcherProductChance, "bonusButcherProductChance", 0.08f);
            Scribe_Values.Look(ref _crematingInjuryChance, "crematingInjuryChance", 0.05f);
            Scribe_Values.Look(ref _electricalRepairFailureChance, "electricalRepairFailureChance", 0.05f);
            Scribe_Values.Look(ref _electricalRepairFireChance, "electricalRepairFireChance", 0.05f);
            Scribe_Values.Look(ref _electricalRepairShortCircuitChance, "_electricalRepairShortCircuitChance", 0.05f);
            Scribe_Values.Look(ref _plantHarvestingFindEggsChance, "plantHarvestingFindEggsChance", 0.05f);
            Scribe_Values.Look(ref _hurtByFarmAnimalChance, "hurtByFarmAnimalChance", 0.05f);
            Scribe_Values.Look(ref _injuredByApparelChance, "injuredByApparelChance", 0.05f);
            Scribe_Values.Look(ref _allowMessages, "allowMessages", true);
            Scribe_Values.Look(ref _allowFilthyRoomEvent, "allowFilthyRoomEvent", true);
            Scribe_Values.Look(ref _filthyRoomCooldownTicks, "filthyRoomCooldownTicks", 420000);
            Scribe_Values.Look(ref _filthyRoomSampleInterval, "filthyRoomSampleInterval", 45000);
            Scribe_Values.Look(ref _filthyRoomSampleChecks, "filthyRoomSampleChecks", 2);
            Scribe_Values.Look(ref _filthyRoomSpawnThreshold, "filthyRoomSpawnThreshold", 12);
            Scribe_Values.Look(ref _filthyRoomSpawnMHChance, "filthyRoomSpawnMHChance", 0.2f);
            Scribe_Values.Look(ref _filthyRoomPestSpawnRange, "filthyRoomPestSpawnRange", new IntRange(1, 10));
        }
    }
}
