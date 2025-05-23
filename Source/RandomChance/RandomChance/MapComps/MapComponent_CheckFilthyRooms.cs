﻿using System.Collections.Generic;
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
        private bool _onCooldown;
        private int _cooldownTicks;
        private int _lastUpdate;
        private readonly Dictionary<Room, int> _roomFilthCounters = new();
        
        public MapComponent_CheckFilthyRooms(Map map) : base(map) { }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (!RCSettings.AllowFilthyRoomEvent) return;
            if (_onCooldown)
            {
                if (--_cooldownTicks <= 0)
                {
                    _onCooldown = false;
                }
                return;
            }
            
            if (map != null && _lastUpdate + RCSettings.FilthyRoomSampleInterval <= Find.TickManager.TicksAbs)
            {
                AnimalSpawnerTick();
            }
        }
        
        private void AnimalSpawnerTick()
        {
            // Create a list to track rooms that need their counters reset
            // We do this to avoid updating our dictionary while iterating through it
            // Otherwise... kablooy
            List<Room> roomsToReset = [];

            // Iterate through each SEPARATE room in the home area
            foreach (Room room in map.areaManager.Home.ActiveCells
                         .Select(cell => cell.GetRoom(map)).Distinct())
            {
                if (room is not { CellCount: > 0 } || 
                    !room.ProperRoom || 
                    room.OpenRoofCount != 0) continue;
                
                // Get the filth counter for this room, initialize it if it doesn't exist yet
                if (!_roomFilthCounters.TryGetValue(room, out int roomFilthCounter))
                {
                    roomFilthCounter = 0;
                }
                
                int totalFilthInRoom = RCMapUtil.CalculateRoomDirtiness(room, map);
                
                // Check if the room is dirty enough
                if (totalFilthInRoom > RCSettings.FilthyRoomSpawnThreshold)
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
                if (roomFilthCounter >= RCSettings.FilthyRoomSampleChecks)
                {
                    roomsToReset.Add(room);
                }
            }
            
            // Reset the counters for rooms that need it
            foreach (Room room in roomsToReset)
            {
                RCSpawningUtil.SpawnFilthyRats(room, RCSettings.FilthyRoomPestSpawnRange.RandomInRange, map);
                _roomFilthCounters[room] = 0;
                
                if (RCSettings.AllowMessages)
                {
                    Messages.Message("RC_DirtyRoomsAndFilthyRats"
                        .Translate(), null, MessageTypeDefOf.NeutralEvent);
                }
            }
            
            if (roomsToReset.Any())
            {
                _onCooldown = true;
                _cooldownTicks = RCSettings.FilthyRoomCooldownTicks;
            }
            
            _lastUpdate = Find.TickManager.TicksAbs;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _cooldownTicks, "cooldownTicks");
            Scribe_Values.Look(ref _onCooldown, "onCooldown");
            Scribe_Values.Look(ref _lastUpdate, "lastUpdate");
        }
    }
}