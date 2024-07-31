using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch(typeof(VoreTracker))]
    public class Patch_VoreTracker
    {
        [HarmonyPatch(nameof(VoreTracker.Tick))]
        [HarmonyPostfix]
        public static void Patch_Tick(VoreTracker __instance)
        {
            AccidentalDigestionManager.Manager.GetTracker(__instance.pawn, false)?.TickCooldown();
        }

        [HarmonyPatch(nameof(VoreTracker.TickRare))]
        [HarmonyPostfix]
        public static void Patch_TickRare(VoreTracker __instance)
        {
            
        }
    }
}