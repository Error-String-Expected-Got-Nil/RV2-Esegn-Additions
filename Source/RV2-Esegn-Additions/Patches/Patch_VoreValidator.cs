using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    // Slight modification to HasFreeCapacityFor to make it return true if the prey is already eaten.
    [HarmonyPatch(typeof(VoreValidator))]
    public class Patch_VoreValidator
    {
        // A MEMORIAL: For the 1.5 hours spent to realize I had forgotten to annotate this with [HarmonyPrefix]
        [HarmonyPatch(nameof(VoreValidator.HasFreeCapacityFor))]
        [HarmonyPrefix]
        public static bool Patch_HasFreeCapacityFor(Pawn predator, Pawn prey, ref bool __result)
        {
            if (predator == null || prey == null) return true;

            var hasEatenPrey = predator.PawnData()?.VoreTracker?.VoreTrackerRecords.Any(record =>
                record.Prey == prey);

            if (hasEatenPrey == true)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}