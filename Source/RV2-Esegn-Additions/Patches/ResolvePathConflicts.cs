using System.Linq;
using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch(typeof(VoreTrackerRecord))]
    public class Patch_VoreTrackerRecord_ResolvePathConflicts
    {
        // Prevents a normal path jump if the started path is already conflicting
        [HarmonyPatch(nameof(VoreTrackerRecord.Initialize))]
        [HarmonyPostfix]
        public static void Patch_Initialize(VoreTrackerRecord __instance)
        {
            if (!RV2_EADD_Settings.eadd.EnableVorePathConflicts) return;
            
            if (ConflictingPathUtils.PathConflictsWithAnyActiveVore(__instance.Predator, __instance.VorePath.def,
                    out _))
            {
                __instance.PathToJumpTo = null;
            }
        }

        [HarmonyPatch(nameof(VoreTrackerRecord.MovePreyToNextStage))]
        [HarmonyPrefix]
        public static void Prefix_MovePreyToNextStage(VoreTrackerRecord __instance)
        {
            if (!RV2_EADD_Settings.eadd.EnableVorePathConflicts || !RV2_EADD_Settings.eadd.EnableAccidentalDigestion)
                return;

            // Prevent path jump if there's an accidental digestion record for the next stage
            if (AccidentalDigestionManager.Manager
                    .GetTracker(__instance.Predator, false)?.Records
                    .Find(record => record.JumpKey == __instance.NextVoreStage?.def.jumpKey) != null)
                __instance.PathToJumpTo = null;
        }
        
        // Technically this doesn't run for the first stage, but there shouldn't be any jumpKeys in the first stage
        // of a path anyways, so it should be fine.
        [HarmonyPatch(nameof(VoreTrackerRecord.MovePreyToNextStage))]
        [HarmonyPostfix]
        public static void Postfix_MovePreyToNextStage(VoreTrackerRecord __instance)
        {
            if (!RV2_EADD_Settings.eadd.EnableVorePathConflicts) return;

            if (RV2_EADD_Settings.eadd.EnableAccidentalDigestion)
            {
                var adrecord = AccidentalDigestionManager.Manager
                    .GetTracker(__instance.Predator, false)?.Records
                    .Find(record => record.JumpKey == __instance.CurrentVoreStage.def.jumpKey);

                if (adrecord != null)
                {
                    adrecord.TryAddNewRecord(__instance);
                    return;
                }
            }
            
            ConflictingPathUtils.CheckAndResolvePathConflicts(__instance);
        }

        public static bool VoreJumpFlag = false;
        // JumpToOtherPath is private so can't use nameof()
        [HarmonyPatch("JumpToOtherPath")]
        [HarmonyPrefix]
        public static void Prefix_JumpToOtherPath()
        {
            VoreJumpFlag = true;
        }
        
        [HarmonyPatch("JumpToOtherPath")]
        [HarmonyPostfix]
        public static void Postfix_JumpToOtherPath()
        {
            VoreJumpFlag = false;
        }
    }

    // When a record is split off, check for the flag that indicates a vore jump happened to create the split, and then
    // check other records on the predator for conflicts, using the jumped-to path as the primary.
    // Only works for automatic jumps, but I don't think the manual jump feature even works? Might need to look into
    // that.
    [HarmonyPatch(typeof(VoreTracker))]
    public class Patch_VoreTracker_ResolvePathConflicts
    {
        [HarmonyPatch(nameof(VoreTracker.SplitOffNewVore))]
        [HarmonyPostfix]
        public static void Patch_SplitOffNewVore(VoreTrackerRecord __result)
        {
            if (!RV2_EADD_Settings.eadd.EnableVorePathConflicts) return;
            if (Patch_VoreTrackerRecord_ResolvePathConflicts.VoreJumpFlag)
            {
                ConflictingPathUtils.CheckAndResolveOtherRecords(__result);
                Patch_VoreTrackerRecord_ResolvePathConflicts.VoreJumpFlag = false;
            }
        }
    }
}