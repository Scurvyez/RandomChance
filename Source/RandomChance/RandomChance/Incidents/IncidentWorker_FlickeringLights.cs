using RandomChance.MapComps;
using RimWorld;
using Verse;

namespace RandomChance
{
    public class IncidentWorker_FlickeringLights : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!ModsConfig.AnomalyActive) return false;
            Map map = (Map)parms.target;
            return !map.GetComponent<MapComponent_CollectThings>().availableLightSources.NullOrEmpty();
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (map == null) return true;
            
            MapComponent_FlickerLightSources mapComp = map.GetComponent<MapComponent_FlickerLightSources>();
            mapComp.FlickLightSources = true;
            return true;
        }
    }
}
