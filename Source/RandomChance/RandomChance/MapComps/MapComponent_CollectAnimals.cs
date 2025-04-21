using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace RandomChance
{
    public class MapComponent_AnimalCollections : MapComponent
    {
        public readonly List<PawnKindDef> EggLayingAnimals = [];
        public readonly List<PawnKindDef> NonEggLayingAnimals = [];
        
        private List<BiomeAnimalRecord> _biomeAnimals { get; }
        
        public MapComponent_AnimalCollections(Map map) : base(map)
        {
            FieldInfo wildAnimalsField = AccessTools.Field(typeof(BiomeDef), "wildAnimals");
            _biomeAnimals = wildAnimalsField.GetValue(map.Biome) as List<BiomeAnimalRecord>;
        }
        
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            
            if (_biomeAnimals is not { Count: >= 0 }) return;
            CollectNativeEggLayingWildAnimals();
            CollectNativeNonEggLayingWildAnimals();
        }
        
        private void CollectNativeEggLayingWildAnimals()
        {
            if (map.Biome == null) return;
            if (_biomeAnimals.NullOrEmpty()) return;
            
            foreach (BiomeAnimalRecord animalRecord in _biomeAnimals)
            {
                PawnKindDef kindDef = animalRecord.animal;
                if (kindDef?.race.GetCompProperties<CompProperties_EggLayer>() != null && 
                    kindDef.race.race.baseBodySize <= 1f && 
                    kindDef.combatPower <= 69f)
                {
                    EggLayingAnimals.Add(kindDef);
                }
            }
        }
        
        private void CollectNativeNonEggLayingWildAnimals()
        {
            if (map.Biome == null) return;
            if (_biomeAnimals.NullOrEmpty()) return;
            
            foreach (BiomeAnimalRecord animalRecord in _biomeAnimals)
            {
                PawnKindDef kindDef = animalRecord.animal;
                if (kindDef?.race.race.baseBodySize <= 1f && 
                    kindDef.combatPower <= 69f)
                {
                    NonEggLayingAnimals.Add(kindDef);
                }
            }
        }
    }
}