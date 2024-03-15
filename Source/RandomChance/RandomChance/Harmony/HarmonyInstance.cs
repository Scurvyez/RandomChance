using System.Reflection;
using Verse;
using HarmonyLib;

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public class RandomChanceMain
    {
        static RandomChanceMain()
        {
            Log.Message("[<color=#4494E3FF>Random Chance</color>] 03/15/2024 " + "<color=#ff8c66>[1.5 Update | Older versions will no longer be maintained.]</color>");

            var harmony = new Harmony("com.randomchance");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
