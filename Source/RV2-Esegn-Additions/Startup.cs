using System.Reflection;
using HarmonyLib;
using Verse;

namespace RV2_Esegn_Additions;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        Harmony.DEBUG = false;
        var harmony = new Harmony("RV2_Esegn_Additions");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}