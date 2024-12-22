using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class GameCondition_FlickeringLights : GameCondition
    {
        public override int TransitionTicks => 360;
        
        private static readonly Color _skyColor = new (0.8f, 0.8f, 0.8f);
        private static readonly Color _shadowColor = new (0f, 0.2f, 0.3f);
        private static readonly Color _overlayColor = new (0f, 0.3f, 0.4f);
        private static readonly float _saturation = 0.75f;
        private static readonly float _glow = 0.05f;
        private static readonly SkyColorSet _skyColors = new (_skyColor, _shadowColor, _overlayColor, _saturation);

        private IncidentDef incidentDef;
        private FlickeringLightsExtension flickeringLightsExtension;
        
        public override void Init()
        {
            base.Init();
            incidentDef = RCDefOf.RC_FlickeringLights;
            flickeringLightsExtension = incidentDef.GetModExtension<FlickeringLightsExtension>();
            Map map = Find.CurrentMap;

            if (flickeringLightsExtension == null) return;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if (pawn.IsColonyMech || !pawn.SpawnedOrAnyParentSpawned || PawnUtility.IsBiologicallyOrArtificiallyBlind(pawn) || !pawn.Awake()) continue;
                pawn.needs.mood.thoughts.memories.TryGainMemory(RCDefOf.RC_ExperiencedFlickeringLights);
            }
        }
        
        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(_glow, _skyColors, 1f, 1f);
        }
    }
}
