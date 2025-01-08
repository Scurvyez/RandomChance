using Verse;

namespace RandomChance
{
    public class MapComponent_TimeKeeping : MapComponent
    {
        public int LastSilverFindTick = 600000;
        public int SilverFindThreshold = 600000;
        
        public MapComponent_TimeKeeping(Map map) : base(map)
        {
            
        }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            if (LastSilverFindTick < SilverFindThreshold)
            {
                LastSilverFindTick++;
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref LastSilverFindTick, "LastSilverFindTick", 600000);
        }
    }
}