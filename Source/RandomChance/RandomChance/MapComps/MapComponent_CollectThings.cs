using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RandomChance.MapComps
{
    public class MapComponent_CollectThings : MapComponent
    {
        public List<ThingDef> possibleEggs = new();
        public List<Thing> availableLightSources = new(5);

        private int _lastUpdate;
        private FlickeringLightsExtension flickeringLightsExtension;
        
        public MapComponent_CollectThings(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            flickeringLightsExtension = RCDefOf.RC_FlickeringLights.GetModExtension<FlickeringLightsExtension>();
            
            CollectNativeWildAnimalEggs();

            if (flickeringLightsExtension == null) return;
            CollectAvailableLightSources();
        }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (flickeringLightsExtension == null) return;
            if (map != null && _lastUpdate + flickeringLightsExtension.lightSourceSampleInterval <= Find.TickManager.TicksAbs)
            {
                availableLightSources.Clear();
                CollectAvailableLightSources();
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastUpdate, "lastUpdate");
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

        private void CollectAvailableLightSources()
        {
            if (map == null) return;

            List<Building> lightSources = map.listerBuildings.allBuildingsColonist
                .Where(building => building.HasComp<CompPowerTrader>() && building.HasComp<CompGlower>())
                .ToList();

            lightSources = lightSources.OrderBy(x => Rand.Value).ToList(); // Shuffle the list randomly

            for (int i = 0; i < flickeringLightsExtension.maxLightSources && i < lightSources.Count; i++)
            {
                availableLightSources.Add(lightSources[i]);
            }
            _lastUpdate = Find.TickManager.TicksAbs;
        }
    }
}
