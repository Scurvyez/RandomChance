using RandomChance.MapComps;
using RimWorld;
using Verse;

namespace RandomChance
{
    public class IncidentWorker_FlickeringLights : IncidentWorker_MakeGameCondition
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms)) return false;
            if (!ModsConfig.AnomalyActive) return false;
            Map map = (Map)parms.target;
            return !map.GetComponent<MapComponent_CollectThings>().availableLightSources.NullOrEmpty();
        }
        
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            base.TryExecuteWorker(parms);
            Map map = (Map)parms.target;
            if (map == null) return true;
            
            MapComponent_FlickerLightSources mapComp = map.GetComponent<MapComponent_FlickerLightSources>();
            mapComp.FlickLightSources = true;
            return true;
        }
    }
}
