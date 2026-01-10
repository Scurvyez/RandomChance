using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RandomChance
{
    public static class HarmonyPatchesUtil
    {
        public static readonly List<ThingDef> FilthOptionsCache =
        [
            ThingDefOf.Filth_Dirt,
            ThingDefOf.Filth_Floordrawing,
            ThingDefOf.Filth_Hair,
            ThingDefOf.Filth_Trash,
            ThingDefOf.Filth_DriedBlood,
            ThingDefOf.Filth_MachineBits,
            ThingDefOf.Filth_ScatteredDocuments
        ];
        
        
    }
}