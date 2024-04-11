using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace RandomChance
{
    public class MapComponent_CollectAnimals : MapComponent
    {
        public List<PawnKindDef> eggLayingAnimals = new();

        public MapComponent_CollectAnimals(Map map) : base(map) { }

        public override void MapGenerated()
        {
            base.MapGenerated();

            FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");
            if (wildAnimalsField.GetValue(map.Biome) is List<BiomeAnimalRecord> biomeSpecificAnimals)
            {
                foreach (BiomeAnimalRecord animalRecord in biomeSpecificAnimals)
                {
                    PawnKindDef kindDef = animalRecord.animal;
                    if (kindDef != null && kindDef.race.GetCompProperties<CompProperties_EggLayer>() != null)
                    {
                        eggLayingAnimals.Add(kindDef);
                    }
                }
                //Log.Message($"Wild, egg-laying animal count: {eggLayingAnimals.Count}");
            }
        }
    }
}
