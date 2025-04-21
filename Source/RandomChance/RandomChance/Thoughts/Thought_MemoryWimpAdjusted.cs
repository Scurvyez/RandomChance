using RimWorld;

namespace RandomChance
{
    public class Thought_MemoryWimpAdjusted : Thought_Memory
    {
        public override int CurStageIndex => pawn.story.traits
            .HasTrait(TraitDefOf.Wimp) ? 1 : 0;
    }
}