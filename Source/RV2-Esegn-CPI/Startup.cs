using System.Reflection;
using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_CPI
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            Harmony harmony = new Harmony("RV2_Esegn_CPI");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            RV2Log.Message("TEST - Cross-path interactions loaded!");
        }
    }
}