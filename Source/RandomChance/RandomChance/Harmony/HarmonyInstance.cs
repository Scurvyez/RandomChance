using System.Reflection;

using Verse;
using HarmonyLib;

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            //Harmony.DEBUG = true;
            var harmony = new Harmony("com.randomchance");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
