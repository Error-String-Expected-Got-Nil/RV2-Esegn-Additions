using System.Reflection;
using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("RV2_Esegn_Additions");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}