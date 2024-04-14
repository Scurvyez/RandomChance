using HarmonyLib;
using System.Reflection;
using Verse;

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public static class RandomChanceMain
    {
        static RandomChanceMain()
        {
            Log.Message("[<color=#4494E3FF>Random Chance</color>] 03/15/2024 " + "<color=#ff8c66>[1.5 Update | Older versions will no longer be maintained.]</color>");

            Harmony harmonyTInstance = new Harmony("com.randomchance");
            harmonyTInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
