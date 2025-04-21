using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RandomChance.MapComps
{
    public class MapComponent_CollectThings : MapComponent
    {
        public readonly List<ThingDef> PossibleEggs = [];
        public readonly List<Thing> AvailableLightSources = new(5);
        
        private int _lastUpdate;
        private FlickeringLightsExtension _flickeringLightsExt;
        
        public MapComponent_CollectThings(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            _flickeringLightsExt = RCDefOf.RC_FlickeringLights
                .GetModExtension<FlickeringLightsExtension>();
            
            CollectNativeWildAnimalEggs();

            if (_flickeringLightsExt == null) return;
            CollectAvailableLightSources();
        }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (_flickeringLightsExt == null) return;
            if (map == null || _lastUpdate + _flickeringLightsExt.lightSourceSampleInterval >
                Find.TickManager.TicksAbs) return;
            
            AvailableLightSources.Clear();
            CollectAvailableLightSources();
        }

        private void CollectNativeWildAnimalEggs()
        {
            MapComponent_AnimalCollections animalCollection = 
                map.GetComponent<MapComponent_AnimalCollections>();

            if (animalCollection.EggLayingAnimals.NullOrEmpty()) return;
            for (int i = 0; i < animalCollection.EggLayingAnimals.Count; i++)
            {
                PawnKindDef kindDef = animalCollection.EggLayingAnimals[i];
                Pawn pawn = PawnGenerator.GeneratePawn(kindDef, null);
                CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
                if (compEggLayer != null)
                {
                    PossibleEggs.Add(compEggLayer.Props.eggFertilizedDef);
                }
            }
        }

        private void CollectAvailableLightSources()
        {
            if (map == null) return;
            
            List<Building> lightSources = map.listerBuildings.allBuildingsColonist
                .Where(RCMapUtil.IsColonyLightSource).ToList();
            lightSources = lightSources.OrderBy(x => Rand.Value).ToList();

            for (int i = 0; i < _flickeringLightsExt.maxLightSources && i < lightSources.Count; i++)
            {
                AvailableLightSources.Add(lightSources[i]);
            }
            _lastUpdate = Find.TickManager.TicksAbs;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastUpdate, "lastUpdate");
        }
    }
}