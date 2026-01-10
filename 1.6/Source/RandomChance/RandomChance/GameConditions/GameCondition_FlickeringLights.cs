using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class GameCondition_FlickeringLights : GameCondition
    {
        public override int TransitionTicks => 360;
        
        private static readonly Color SkyColor = new (0.8f, 0.8f, 0.8f);
        private static readonly Color ShadowColor = new (0f, 0.2f, 0.3f);
        private static readonly Color OverlayColor = new (0f, 0.3f, 0.4f);
        private static readonly float Saturation = 0.75f;
        private static readonly float Glow = 0.05f;
        private static readonly SkyColorSet SkyColors = 
            new (SkyColor, ShadowColor, OverlayColor, Saturation);
        
        private IncidentDef _incidentDef;
        private FlickeringLightsExtension _flickeringLightsExtension;
        
        public override void Init()
        {
            base.Init();
            _incidentDef = RCDefOf.RC_FlickeringLights;
            _flickeringLightsExtension = _incidentDef
                .GetModExtension<FlickeringLightsExtension>();
            Map map = Find.CurrentMap;
            
            if (_flickeringLightsExtension == null) return;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if (pawn.IsColonyMech || 
                    !pawn.SpawnedOrAnyParentSpawned || 
                    PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) || 
                    !pawn.Awake()) continue;
                
                pawn.needs.mood.thoughts.memories
                    .TryGainMemory(RCDefOf.RC_ExperiencedFlickeringLights);
            }
        }
        
        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }
        
        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(Glow, SkyColors, 1f, 1f);
        }
    }
}