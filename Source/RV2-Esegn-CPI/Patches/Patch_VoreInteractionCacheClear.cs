using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_CPI
{
    // VoreInteractions are cached, and only updated on occasion. This is *usually* fine, but with the addition of path
    // conflicts, means they need to be cleared whenever a path updates in a way that might change how it conflicts
    // with other paths. I believe this should be:
    //  - When it starts
    //  - When it passes to a new stage
    //  - When it ends
    //  - When a path jump occurs
    // This file has all of these patches.
    
    [HarmonyPatch(typeof(VoreTrackerRecord))]
    public class Patch_VoreTrackerRecord
    {
        // On start and stage pass
        // Path jumps always happen during the stage pass function so they're covered here too 
        [HarmonyPatch(nameof(VoreTrackerRecord.Initialize))]
        [HarmonyPatch(nameof(VoreTrackerRecord.MovePreyToNextStage))]
        [HarmonyPostfix]
        public static void Patch_VoreTrackerRecordClearCache()
        {
            if (RV2_CPI_Settings.cpi.EnableVorePathConflicts) VoreInteractionManager.ClearCachedInteractions();
        }
    }

    [HarmonyPatch(typeof(VoreTracker))]
    public class Patch_VoreTracker
    {
        // On path end
        [HarmonyPatch(nameof(VoreTracker.UntrackVore))]
        [HarmonyPostfix]
        public static void Patch_VoreTrackerClearCache()
        {
            if (RV2_CPI_Settings.cpi.EnableVorePathConflicts) VoreInteractionManager.ClearCachedInteractions();
        }
    }
}