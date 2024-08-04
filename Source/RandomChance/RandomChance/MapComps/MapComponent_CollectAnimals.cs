using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace RandomChance
{
    public class MapComponent_AnimalCollections : MapComponent
    {
        public List<PawnKindDef> eggLayingAnimals = new();

        public MapComponent_AnimalCollections(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CollectNativeEggLayingWildAnimals();
        }

        private void CollectNativeEggLayingWildAnimals()
        {
            if (map.Biome == null) return;
            FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");

            if (wildAnimalsField.GetValue(map.Biome) is not List<BiomeAnimalRecord> biomeSpecificAnimals ||
                biomeSpecificAnimals.NullOrEmpty()) return;
            
            foreach (BiomeAnimalRecord animalRecord in biomeSpecificAnimals)
            {
                PawnKindDef kindDef = animalRecord.animal;
                if (kindDef?.race.GetCompProperties<CompProperties_EggLayer>() != null
                    && kindDef.race.race.baseBodySize <= 1f
                    && kindDef.combatPower <= 75f)
                {
                    eggLayingAnimals.Add(kindDef);
                }
            }
        }
    }
}
