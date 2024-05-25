using RimWorld;
using Verse;

namespace RandomChance
{
    public static class RCSpawningUtil
    {
        public static void SpawnFilthyRats(Room room, int numToSpawn, Map map, float manhuntChance)
        {
            PawnKindDef animalKindDef = RCDefOf.Rat;
            IntVec3 spawnCell = room.Cells.RandomElement();

            for (int i = 0; i < numToSpawn; i++)
            {
                Pawn animalToSpawn = PawnGenerator.GeneratePawn(animalKindDef, null);
                GenSpawn.Spawn(animalToSpawn, spawnCell, map);

                if (Rand.Value < manhuntChance)
                {
                    animalToSpawn.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                }
            }
        }
    }
}
