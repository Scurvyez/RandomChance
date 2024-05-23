using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RandomChance
{
    /// <summary>
    /// Documented the hell out of this one cause it took me a bit to get right,
    /// and I'll probably never remember how the fuck it works otherwise.
    /// </summary>
    public class MapComponent_CheckFilthyRooms : MapComponent
    {
        private int _lastUpdate;
        private int _countToSpawn = Rand.RangeInclusive(1, 5);
        private Dictionary<Room, int> _roomFilthCounters = new();
        
        private const int SampleDuration = 720; // (45,000)... 3/4 of a day?
        private const int FilthThreshold = 6; // 6 pieces of filth?
        private const int FilthChecksLimit = 2; // 2 checks till rats!?
        private const float ManhuntChance = 0.3f;

        public MapComponent_CheckFilthyRooms(Map map) : base(map) { }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (map != null && _lastUpdate + SampleDuration <= Find.TickManager.TicksAbs)
            {
                AnimalSpawnerTick();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastUpdate, "lastUpdate");
        }

        private void AnimalSpawnerTick()
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
                    if (!_roomFilthCounters.TryGetValue(room, out var roomFilthCounter))
                    {
                        roomFilthCounter = 0;
                    }

                    int totalFilthInRoom = CalculateRoomDirtiness(room);
                    //RCLog.Message($"Filth in {room.ID}: {totalFilthInRoom}");

                    // Check if the room is dirty enough
                    if (totalFilthInRoom > FilthThreshold)
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
                    _roomFilthCounters[room] = roomFilthCounter;

                    // Check if this room needs its counter reset
                    if (roomFilthCounter >= FilthChecksLimit)
                    {
                        roomsToReset.Add(room);
                    }
                }
            }

            // Reset the counters for rooms that need it
            foreach (var room in roomsToReset)
            {
                SpawnFilthyRats(room);
                _roomFilthCounters[room] = 0;

                if (RCSettings.AllowMessages)
                {
                    Messages.Message("RC_DirtyRoomsAndFilthyRats".Translate(), null, MessageTypeDefOf.NeutralEvent);
                }
            }

            _lastUpdate = Find.TickManager.TicksAbs;
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
            PawnKindDef animalKindDef = RCDefOf.Rat;
            IntVec3 spawnCell = room.Cells.RandomElement();

            for (int i = 0; i < _countToSpawn; i++)
            {
                Pawn animalToSpawn = PawnGenerator.GeneratePawn(animalKindDef, null);
                GenSpawn.Spawn(animalToSpawn, spawnCell, map);

                if (Rand.Value < ManhuntChance)
                {
                    animalToSpawn.mindState?.mentalStateHandler?.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
                }
            }
        }
    }
}
