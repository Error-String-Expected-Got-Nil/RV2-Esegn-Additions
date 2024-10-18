using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(Window_Quirks))]
public class Patch_Window_Quirks
{
    [HarmonyPatch(nameof(Window_Quirks.Close))]
    [HarmonyPostfix]
    public static void Patch_Close(Window_Quirks __instance)
    {
        var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

        // Pawn should never be null, but just in case...
        if (pawn != null)
        {
            AccidentalDigestionManager.Manager.GetTracker(pawn, false)?.UpdateModifierCache();
        }
    }
}