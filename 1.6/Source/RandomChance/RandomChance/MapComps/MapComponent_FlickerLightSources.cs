using RandomChance.MapComps;
using RimWorld;
using Verse;

namespace RandomChance
{
    public class MapComponent_FlickerLightSources : MapComponent
    {
        public bool FlickLightSources;

        private int _counter = 0;
        private FlickeringLightsExtension _flickeringLightsExtension;
        
        public MapComponent_FlickerLightSources(Map map) : base(map) { }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            _flickeringLightsExtension = RCDefOf.RC_FlickeringLights.GetModExtension<FlickeringLightsExtension>();
        }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            
            if (_flickeringLightsExtension == null) return;
            if (!FlickLightSources) return;
            
            _counter++;
            if (_counter <= _flickeringLightsExtension.flickerDuration)
            {
                TryFlickLightSourcesOnOff();
            }
            else
            {
                FlickLightSources = false;
                _counter = 0;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FlickLightSources, "FlickLightSources");
        }
        
        private void TryFlickLightSourcesOnOff()
        {
            MapComponent_CollectThings mapComp = map?.GetComponent<MapComponent_CollectThings>();
            if (mapComp == null) return;

            foreach (Thing thing in mapComp.AvailableLightSources)
            {
                CompPowerTrader comp = thing.TryGetComp<CompPowerTrader>();
                if (comp != null 
                    && Rand.Chance(_flickeringLightsExtension.flickerChance) 
                    && thing.IsHashIntervalTick(60))
                {
                    comp.PowerOn = !comp.PowerOn;
                }
            }
        }
    }
}
