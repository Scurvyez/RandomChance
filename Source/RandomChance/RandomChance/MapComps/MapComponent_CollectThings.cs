using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RandomChance.MapComps
{
    public class MapComponent_CollectThings : MapComponent
    {
        public List<ThingDef> possibleEggs = new();

        public MapComponent_CollectThings(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            CollectNativeWildAnimalEggs();
        }

        private void CollectNativeWildAnimalEggs()
        {
            MapComponent_AnimalCollections animalCollection = map.GetComponent<MapComponent_AnimalCollections>();

            if (!animalCollection.eggLayingAnimals.NullOrEmpty())
            {
                for (int i = 0; i < animalCollection.eggLayingAnimals.Count; i++)
                {
                    PawnKindDef kindDef = animalCollection.eggLayingAnimals[i];
                    Pawn pawn = PawnGenerator.GeneratePawn(kindDef, null);
                    CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
                    if (compEggLayer != null)
                    {
                        possibleEggs.Add(compEggLayer.Props.eggFertilizedDef);
                    }
                }
            }
        }
    }
}
