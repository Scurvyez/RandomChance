using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace RandomChance
{
    public static class RCFailureUtil
    {
        public static void StartFireHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (RCSettings.AllowMessages)
            {
                Messages.Message("RC_FireInKitchen"
                    .Translate(actor.Named("PAWN")),
                    actor, MessageTypeDefOf.NegativeEvent);
            }

            Thing ingredients = curJob.GetTarget(TargetIndex.B).Thing;
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;

            if (ingredients == null) return;
            if (!ingredients.Destroyed)
            {
                ingredients.Destroy();
            }

            FireUtility.TryStartFireIn(buildingPos, map, RCSettings.FailedCookingFireSize, null);
            MoteMaker.MakeColonistActionOverlay(actor, ThingDefOf.Mote_ColonistFleeing);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            actor.stances.stunner.StunFor(120, actor, false, false);
        }
        
        public static void GiveInjuryHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            int pawnsAvgSkillLevel = (int)actor.skills
                .AverageOfRelevantSkillsFor(actor.CurJob.workGiverDef.workType);
            IntVec3 buildingPos = building.Position;
            Map map = building.Map;
            HediffDef burnHediffDef = RCDefOf.Burn;
            HediffDef cutHediffDef = HediffDefOf.Cut;

            float severity = pawnsAvgSkillLevel switch
            {
                < 5 => 0.4f,
                <= 10 => 0.2f,
                <= 15 => 0.08f,
                <= 18 => 0.02f,
                <= 20 => 0f,
                _ => 0f,
            };
            
            if (curJob.RecipeDef.defName.IndexOf("Cook", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                BodyPartRecord fingersPart = actor.RaceProps.body
                    .GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit).RandomElement();

                if (fingersPart == null) return;
                if (Rand.Bool)
                {
                    Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, fingersPart);
                    hediff.Severity = severity;
                    actor.health.AddHediff(hediff);

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryInKitchen"
                                .Translate(actor.Named("PAWN")),
                            actor, MessageTypeDefOf.NegativeEvent);
                    }
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(cutHediffDef, actor, fingersPart);
                    hediff.Severity = severity;
                    actor.health.AddHediff(hediff);

                    if (RCSettings.AllowMessages)
                    {
                        Messages.Message("RC_InjuryInKitchen"
                                .Translate(actor.Named("PAWN")),
                            actor, MessageTypeDefOf.NegativeEvent);
                    }

                    IntVec3 adjacentCell = buildingPos + GenAdj.CardinalDirections.RandomElement();
                    FilthMaker.TryMakeFilth(adjacentCell, map, ThingDefOf.Filth_Blood);
                }
            }
            else if (curJob.RecipeDef == RCDefOf.CremateCorpse)
            {
                BodyPartRecord bodyPart = actor.RaceProps.body
                    .GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbSegment).RandomElement();

                if (bodyPart == null) return;
                Hediff hediff = HediffMaker.MakeHediff(burnHediffDef, actor, bodyPart);
                hediff.Severity = severity;
                actor.health.AddHediff(hediff);

                if (RCSettings.AllowMessages)
                {
                    Messages.Message("RC_InjuryWhileCremating"
                            .Translate(actor.Named("PAWN")),
                        actor, MessageTypeDefOf.NegativeEvent);
                }
            }
        }
        
        public static void CauseMessHandler(Pawn actor, Job curJob, Building_WorkTable building)
        {
            if (curJob.GetTarget(TargetIndex.B).Thing is not Corpse animalCorpse ||
                building == null ||
                actor == null) return;
            
            IntVec3 pawnPos = actor.Position;
            Map map = building.Map;
            int radius = RCSettings.ButcherMessRadius;
            IntVec3 centerCell = pawnPos + GenAdj.CardinalDirections.RandomElement();
            Pawn animalPawn = animalCorpse.InnerPawn;
            Region region = centerCell.GetRegion(map);
            
            if (region != null)
            {
                foreach (IntVec3 cell in GenRadial.RadialCellsAround(centerCell, radius, true))
                {
                    if (Equals(cell.GetRegion(map), region))
                    {
                        FilthMaker.TryMakeFilth(cell, map, animalPawn.def.race.BloodDef);
                    }
                }
            }
            else
            {
                FilthMaker.TryMakeFilth(centerCell, map, animalPawn.def.race.BloodDef);
            }

            if (RCSettings.AllowMessages)
            {
                Messages.Message("RC_HorriblyUncleanKitchen"
                        .Translate(actor.Named("PAWN")),
                    actor, MessageTypeDefOf.NegativeEvent);
            }
        }
    }
}