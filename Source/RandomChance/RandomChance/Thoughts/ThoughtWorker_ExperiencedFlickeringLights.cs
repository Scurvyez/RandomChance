using RimWorld;
using Verse;

namespace RandomChance
{
    public class ThoughtWorker_ExperiencedFlickeringLights : ThoughtWorker_GameCondition
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return base.CurrentStateInternal(p).Active 
                   && p.SpawnedOrAnyParentSpawned 
                   && !PawnUtility.IsBiologicallyOrArtificiallyBlind(p)
                   && p.Awake();
        }
    }
}
