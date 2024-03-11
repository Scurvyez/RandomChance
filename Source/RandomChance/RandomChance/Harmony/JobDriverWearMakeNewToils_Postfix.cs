using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RandomChance
{
    [HarmonyPatch(typeof(JobDriver_Wear), "MakeNewToils")]
    public class JobDriverWearMakeNewToils_Postfix
    {
        private static List<BodyPartRecord> digits = new List<BodyPartRecord>();

        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<Toil> __result, JobDriver_Wear __instance)
        {
            digits = __instance.pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ManipulationLimbDigit);

            List<Toil> newToils = new(__result);
            int numToils = newToils.Count;

            // new toil with delegate
            Toil customToil = new Toil
            {
                initAction = delegate
                {
                    if (!__instance.pawn.IsColonyMech && RandomChance_DefOf.RC_Curves != null)
                    {
                        DamageDef damageInflicted = DamageDefOf.Cut;
                        SimpleCurve injuryCurve = RandomChance_DefOf.RC_Curves.powerArmorInjuryCurve;
                        float workerInjuryChance = RandomChanceSettings.InjuredByApparelChance;
                        float pawnsSkillLevel = __instance.pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
                        float qualityFactor = QualityGetter.GetQualityValue(__instance.job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompQuality>().Quality, injuryCurve);

                        if (Rand.Chance(workerInjuryChance))
                        {
                            if (Rand.Chance(injuryCurve.Evaluate(pawnsSkillLevel)))
                            {
                                float damageAmount = Rand.Range(0.1f, pawnsSkillLevel) * qualityFactor;

                                if (__instance.job.GetTarget(TargetIndex.A).Thing.def == RandomChance_DefOf.Apparel_PowerArmor) // body
                                {
                                    DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, digits.RandomElement());
                                    __instance.pawn.TakeDamage(damageInfo);
                                }
                                if (__instance.job.GetTarget(TargetIndex.A).Thing.def == RandomChance_DefOf.Apparel_PowerArmorHelmet) // head
                                {
                                    DamageInfo damageInfo = new(damageInflicted, damageAmount, 1f, -1f, null, __instance.pawn.RaceProps.body.GetPartsWithDef(BodyPartDefOf.Head).RandomElement());
                                    __instance.pawn.TakeDamage(damageInfo);
                                }
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
}
