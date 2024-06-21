using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RandomChance
{
    /*public class JobGiver_SwitchFlickingSpree : ThinkNode_JobGiver
    {
        private IntRange waitBetweenTicks = new IntRange(80, 140);
        private static List<Thing> flickableBuildings = new ();

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_SwitchFlickingSpree obj = (JobGiver_SwitchFlickingSpree)base.DeepCopy(resolve);
            obj.waitBetweenTicks = waitBetweenTicks;
            return obj;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            float switchFlickingSpreeChance = RCSettings.SwitchFlickingSpreeChance;
            SimpleCurve switchFlickingSpreeCurve = RCDefOf.RC_ConfigCurves.switchFlickingSpreeCurve;
            float pawnsAvgSkillLevel = pawn.skills.GetSkill(SkillDefOf.Social).Level;

            if (pawn.mindState.nextMoveOrderIsWait)
            {
                Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
                job.expiryInterval = waitBetweenTicks.RandomInRange;
                pawn.mindState.nextMoveOrderIsWait = false;
                return job;
            }
            if (Rand.Chance(switchFlickingSpreeChance))
            {
                if (Rand.Chance(switchFlickingSpreeCurve.Evaluate(pawnsAvgSkillLevel)))
                {
                    Thing thing = TryFindFlickableTarget(pawn);
                    if (thing != null)
                    {
                        pawn.mindState.nextMoveOrderIsWait = true;
                        Log.Message($"Thing being flicked is: {thing}");
                        return JobMaker.MakeJob(JobDefOf.Deconstruct, thing);
                    }
                }
            }
            IntVec3 intVec = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 10f, null, Danger.Deadly);
            if (intVec.IsValid)
            {
                pawn.mindState.nextMoveOrderIsWait = true;
                return JobMaker.MakeJob(JobDefOf.GotoWander, intVec);
            }
            return null;
        }

        private Thing TryFindFlickableTarget(Pawn pawn)
        {
            if (!CellFinder.TryFindClosestRegionWith(pawn.GetRegion(), TraverseParms.For(pawn), (Region candidateRegion) => !candidateRegion.IsForbiddenEntirely(pawn), 100, out var result))
            {
                return null;
            }
            flickableBuildings.Clear();
            List<Thing> allThings = result.ListerThings.AllThings;
            for (int i = 0; i < allThings.Count; i++)
            {
                Thing thing = allThings[i];
                if (thing.def.category == ThingCategory.Building && thing.TryGetComp<CompFlickable>() != null)
                {
                    flickableBuildings.Add(thing);
                }
            }
            if (flickableBuildings.NullOrEmpty())
            {
                return null;
            }
            return flickableBuildings.RandomElement();
        }
    }*/
}
