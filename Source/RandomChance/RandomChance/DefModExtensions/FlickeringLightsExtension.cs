using Verse;

namespace RandomChance
{
    public class FlickeringLightsExtension : DefModExtension
    {
        public int flickerDuration = 2000;
        public int maxLightSources = 1;
        public float flickerChance = 1f;
        public int lightSourceSampleInterval = 2000;
    }
}
