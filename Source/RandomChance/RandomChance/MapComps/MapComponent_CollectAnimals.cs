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
            if (map.Biome != null)
            {
                FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");
                if (wildAnimalsField.GetValue(map.Biome) is List<BiomeAnimalRecord> biomeSpecificAnimals && !biomeSpecificAnimals.NullOrEmpty())
                {
                    foreach (BiomeAnimalRecord animalRecord in biomeSpecificAnimals)
                    {
                        if (animalRecord.animal != null)
                        {
                            PawnKindDef kindDef = animalRecord.animal;
                            if (kindDef != null && kindDef.race.GetCompProperties<CompProperties_EggLayer>() != null)
                            {
                                eggLayingAnimals.Add(kindDef);
                            }
                        }
                    }
                }
            }
        }
    }
}
