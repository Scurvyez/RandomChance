using HarmonyLib;
using System.Reflection;
using Verse;

namespace RandomChance
{
    [StaticConstructorOnStartup]
    public static class RCMain
    {
        static RCMain()
        {
            RCLog.Message("1.5 Update | Older versions will no longer be maintained.");

            Harmony harmonyTInstance = new ("com.randomchance");
            harmonyTInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
