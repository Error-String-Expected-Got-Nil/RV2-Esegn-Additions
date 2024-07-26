using HarmonyLib;
using RimVore2;
using RV2_Esegn_CPI.Utilities;

namespace RV2_Esegn_CPI
{
    [HarmonyPatch(typeof(VoreTrackerRecord))]
    public class Patch_VoreTrackerRecord
    {
        // Prevents a normal path jump if the started path is already conflicting
        [HarmonyPatch(nameof(VoreTrackerRecord.Initialize))]
        [HarmonyPostfix]
        public static void Patch_Initialize(VoreTrackerRecord __instance)
        {
            if (!RV2_CPI_Settings.cpi.EnableVorePathConflicts) return;
            
            if (ConflictingPathUtils.PathConflictsWithAnyActiveVore(__instance.Predator, __instance.VorePath.def,
                    out _))
            {
                __instance.PathToJumpTo = null;
            }
        }

        // Technically this doesn't run for the first stage, but there shouldn't be any jumpKeys in the first stage
        // of a path anyways, so it should be fine.
        [HarmonyPatch(nameof(VoreTrackerRecord.MovePreyToNextStage))]
        [HarmonyPostfix]
        public static void Patch_MovePreyToNextStage(VoreTrackerRecord __instance)
        {
            if (!RV2_CPI_Settings.cpi.EnableVorePathConflicts) return;
            ConflictingPathUtils.CheckAndResolvePathConflicts(__instance);
        }

        public static bool VoreJumpFlag = false;
        // JumpToOtherPath is private so can't use nameof()
        [HarmonyPatch("JumpToOtherPath")]
        [HarmonyPrefix]
        public static void Patch_JumpToOtherPath_Prefix()
        {
            VoreJumpFlag = true;
        }

        // Failsafe, shouldn't be necessary but just in case.
        [HarmonyPatch("JumpToOtherPath")]
        [HarmonyPostfix]
        public static void Patch_JumpToOtherPath_Postfix()
        {
            VoreJumpFlag = false;
        }
    }

    // When a record is split off, check for the flag that indicates a vore jump happened to create the split, and then
    // check other records on the predator for conflicts, using the jumped-to path as the primary.
    // Only works for automatic jumps, but I don't think the manual jump feature even works? Might need to look into
    // that.
    [HarmonyPatch(typeof(VoreTracker))]
    public class Patch_VoreTracker
    {
        [HarmonyPatch(nameof(VoreTracker.SplitOffNewVore))]
        [HarmonyPostfix]
        public static void Patch_SplitOffNewVore(VoreTrackerRecord __result)
        {
            if (Patch_VoreTrackerRecord.VoreJumpFlag)
            {
                ConflictingPathUtils.CheckAndResolveOtherRecords(__result);
                Patch_VoreTrackerRecord.VoreJumpFlag = false;
            }
        }
    }
}