using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    /// <summary>
    /// Documented the hell out of this one cause it took me a bit to get right and I'll
    /// probably never remember how the fuck it works otherwise.
    /// </summary>
    public class MapComponent_CheckFilthyRooms : MapComponent
    {
        private int lastUpdate;
        private int SampleDuration = 45000; // 3/4 of a day?
        private int filthThreshold = 6; // 6 pieces of filth?
        private int filthChecksLimit = 2; // 2 checks till rats!?
        private Dictionary<Room, int> roomFilthCounters = new();

        public MapComponent_CheckFilthyRooms(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (map != null && lastUpdate + SampleDuration <= Find.TickManager.TicksAbs)
            {
                AnimalSpawnerTick();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastUpdate, "lastUpdate");
        }

        public void AnimalSpawnerTick()
        {
            // Create a list to track rooms that need their counters reset
            // We do this to avoid updating our dictionary while iterating through it
            // Otherwise... kablooy
            List<Room> roomsToReset = new();

            // Iterate through each SEPARATE room in the home area
            foreach (Room room in map.areaManager.Home.ActiveCells.Select(cell => cell.GetRoom(map)).Distinct())
            {
                if (room != null && room.CellCount > 0 && room.ProperRoom && room.OpenRoofCount == 0)
                {
                    // Get the filth counter for this room, initialize it if it doesn't exist yet
                    if (!roomFilthCounters.TryGetValue(room, out int roomFilthCounter))
                    {
                        roomFilthCounter = 0;
                    }

                    int totalFilthInRoom = CalculateRoomDirtiness(room);
                    //Log.Message($"Filth in {room.ID}: {totalFilthInRoom}");

                    // Check if the room is dirty enough
                    if (totalFilthInRoom > filthThreshold)
                    {
                        // Increment the filth counter for this room if it is
                        roomFilthCounter++;
                    }
                    else
                    {
                        // Decrement the filth counter for this room (with a lower limit of 0) if it isn't
                        roomFilthCounter = Mathf.Max(0, roomFilthCounter - 1);
                    }

                    // Update the filth counter for this room
                    roomFilthCounters[room] = roomFilthCounter;

                    // Check if this room needs its counter reset
                    if (roomFilthCounter >= filthChecksLimit)
                    {
                        roomsToReset.Add(room);
                    }
                }
            }

            // Reset the counters for rooms that need it
            foreach (var room in roomsToReset)
            {
                SpawnFilthyRats(room);
                roomFilthCounters[room] = 0;

                if (RandomChanceSettings.AllowMessages)
                {
                    Messages.Message("RC_DirtyRoomsAndFilthyRats".Translate(), null, MessageTypeDefOf.NeutralEvent);
                }
            }

            lastUpdate = Find.TickManager.TicksAbs;
        }

        private int CalculateRoomDirtiness(Room room)
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

        private void SpawnFilthyRats(Room room)
        {
            PawnKindDef animalKindDef = RandomChance_DefOf.Rat;
            int numberToSpawn = Rand.RangeInclusive(1, 5);
            IntVec3 spawnCell = room.Cells.RandomElement();

            for (int i = 0; i < numberToSpawn; i++)
            {
                Pawn animalToSpawn = PawnGenerator.GeneratePawn(animalKindDef, null);
                GenSpawn.Spawn(animalToSpawn, spawnCell, map);
            }
        }
    }
}
