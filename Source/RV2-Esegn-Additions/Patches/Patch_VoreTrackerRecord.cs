using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch(typeof(VoreTrackerRecord))]
    public class Patch_VoreTrackerRecord
    {
        // Patch is low priorty to make sure it executes after the ResolvePathConflicts patch on this same method.
        // Made more organizational sense to put them in separate files. At least at the time. It might be messy
        // at the end of all this but... whatever, man. If it works, it works.
        [HarmonyPatch(nameof(VoreTrackerRecord.MovePreyToNextStage))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void Patch_MovePreyToNextStage(VoreTrackerRecord __instance)
        {
            AccidentalDigestionManager.Manager.GetTracker(__instance.Predator).UpdateRecord(__instance);
        }
    }
}