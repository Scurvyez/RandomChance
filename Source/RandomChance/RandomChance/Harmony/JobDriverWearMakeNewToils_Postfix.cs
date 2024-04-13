using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RandomChance
{
    /*
    [HarmonyPatch(typeof(JobDriver_Wear), "MakeNewToils")]
    public class JobDriverWearMakeNewToils_Postfix
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_Wear __instance)
        {
            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        DamageDef damageInflicted = DamageDefOf.Cut;
                        SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.powerArmorInjuryCurve;
                        float pawnsSkillLevel = __instance.pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
                        float qualityFactor = QualityGetter.GetQualityValue(__instance.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompQuality>().Quality, injuryCurve);
                        List<BodyPartRecord> digits = __instance.pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit);
                        ThingDef apparelPiece = __instance.job.GetTarget(TargetIndex.A).Thing.def;

                        if (Rand.Chance(RandomChanceSettings.InjuredByApparelChance) && Rand.Chance(injuryCurve.Evaluate(pawnsSkillLevel)))
                        {
                            float damageAmount = Rand.Range(0.1f, pawnsSkillLevel) * qualityFactor;

                            if (apparelPiece == RandomChance_DefOf.Apparel_PowerArmor) // body
                            {
                                DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, digits.RandomElement());
                                __instance.pawn.TakeDamage(damageInfo);
                            }
                            else if (apparelPiece == RandomChance_DefOf.Apparel_PowerArmorHelmet) // head
                            {
                                DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, __instance.pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head).RandomElement());
                                __instance.pawn.TakeDamage(damageInfo);
                            }

                            if (RandomChanceSettings.AllowMessages)
                            {
                                Messages.Message("RC_InjuryWhileDonningArmor".Translate(__instance.pawn.Named("PAWN"),
                                    apparelPiece.label), __instance.pawn, MessageTypeDefOf.NegativeEvent);
                            }
                        }
                    }
                }
            };

            // insert new toil before the last one in the list
            int insertIndex = newToils.Count - 1;
            newToils.Insert(insertIndex, customToil);

            __result = newToils;
        }
    }
    */
}
