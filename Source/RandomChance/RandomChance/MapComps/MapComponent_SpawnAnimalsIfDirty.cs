using System.Collections.Generic;
using Verse;
using RimWorld;

namespace RandomChance
{
    public class MapComponent_SpawnAnimalsIfDirty : MapComponent
    {
        public int dirtinessThreshold = 6;
        private HashSet<IntVec3> foodCells = new HashSet<IntVec3>();

        public MapComponent_SpawnAnimalsIfDirty(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            foreach (IntVec3 cell in map.AllCells)
            {
                if (IsCellInStockpileZone(cell))
                {
                    if (CellContainsFood(cell))
                    {
                        int dirtinessLevel = CalculateDirtiness(cell);

                        if (dirtinessLevel > dirtinessThreshold)
                        {
                            SpawnAnimals(cell);

                            if (RandomChanceSettings.AllowMessages)
                            {
                                Messages.Message("RC_DirtyFoodStoresArea", MessageTypeDefOf.NeutralEvent);
                            }
                        }
                    }
                }
            }
        }

        private bool IsCellInStockpileZone(IntVec3 cell)
        {
            Zone zone = map.zoneManager.ZoneAt(cell);
            return zone != null && zone is Zone_Stockpile;
        }

        private bool CellContainsFood(IntVec3 cell)
        {
            if (foodCells.Contains(cell))
            {
                return true;
            }

            List<Thing> things = cell.GetThingList(map);
            foreach (Thing thing in things)
            {
                if (thing.def.IsIngestible)
                {
                    foodCells.Add(cell);
                    return true;
                }
            }
            return false;
        }

        private int CalculateDirtiness(IntVec3 cell)
        {
            List<Thing> things = cell.GetThingList(map);
            int dirtinessLevel = 0;

            foreach (Thing thing in things)
            {
                if (thing.def.IsFilth)
                {
                    dirtinessLevel++;
                }
            }
            return dirtinessLevel;
        }

        private void SpawnAnimals(IntVec3 cell)
        {
            PawnKindDef animalKindDef = RandomChance_DefOf.Rat;

            int numberToSpawn = Rand.RangeInclusive(1, 15);

            for (int i = 0; i < numberToSpawn; i++)
            {
                Pawn animalToSpawn = PawnGenerator.GeneratePawn(animalKindDef, null);
                GenSpawn.Spawn(animalToSpawn, cell, map);
            }
        }
    }
}
