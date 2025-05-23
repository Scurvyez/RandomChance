using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RandomChance
{
    public static class RCMapUtil
    {
        public static int CalculateRoomDirtiness(Room room, Map map)
        {
            int dirtinessLevel = 0;
            foreach (IntVec3 cell in room.Cells)
            {
                List<Thing> things = cell.GetThingList(map);
                foreach (Thing thing in things)
                {
                    if (thing.def.IsFilth)
                    {
                        dirtinessLevel++;
                    }
                }
            }
            return dirtinessLevel;
        }
        
        public static bool IsColonyLightSource(Building building)
        {
            return building.HasComp<CompPowerTrader>() && building.HasComp<CompGlower>();
        }
    }
}