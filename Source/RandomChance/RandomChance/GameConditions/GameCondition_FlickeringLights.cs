using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    public class GameCondition_FlickeringLights : GameCondition
    {
        private static readonly Color _skyColor = new (0.1f, 0.1f, 0.1f);
        private static readonly Color _shadowColor = new (0.5f, 0.1f, 0.1f);
        private static readonly Color _overlayColor = new (0.5f, 0.1f, 0.1f);
        private static readonly float _saturation = 0.75f;
        private static readonly float _glow = 0.05f;
        
        public override int TransitionTicks => 360;
        public static readonly SkyColorSet TestSkyColors = new (_skyColor, _shadowColor, _overlayColor, _saturation);

        public override float SkyTargetLerpFactor(Map map)
        {
            return GameConditionUtility.LerpInOutValue(this, TransitionTicks);
        }

        public override SkyTarget? SkyTarget(Map map)
        {
            return new SkyTarget(_glow, TestSkyColors, 1f, 1f);
        }
    }
}
