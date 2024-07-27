using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;
using Verse;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch(typeof(VorePathDef))]
    public class Patch_VorePathDef
    {
        public static bool DisablePathConflictChecks = false; 
        
        [HarmonyPatch(nameof(VorePathDef.IsValid))]
        [HarmonyPostfix]
        public static void Postfix_IsValid(Pawn predator, Pawn prey, out string reason, bool isForAuto, 
            bool ignoreDesignations, bool ignoreRules, VorePathDef __instance, ref bool __result)
        {
            reason = null;

            if (DisablePathConflictChecks) return;
            
            if (!RV2_EsegnAdditions_Settings.eadd.EnableVorePathConflicts) return;
            
            if (ConflictingPathUtils.PathConflictsWithAnyActiveVore(predator, __instance, out var conflictingRecord))
            {
                if (!RV2_EsegnAdditions_Settings.eadd.PathConflictsIgnoreDesignations
                    // If conflicts obey designations, prevent the path if it would cause a conflict that would resolve
                    // in a way that disobeys designations...
                    && (
                        (conflictingRecord.VoreGoal.IsLethal
                        && prey.PawnData()?.Designations?.TryGetValue(RV2DesignationDefOf.fatal)?.IsEnabled() == false)
                        ||
                        (!conflictingRecord.VoreGoal.IsLethal
                        && prey.PawnData()?.Designations?.TryGetValue(RV2DesignationDefOf.endo)?.IsEnabled() == false)
                        ) 
                    // ...unless the predator is a goal-switcher, and the base mod setting that allows goal-switchers
                    // to ignore designations is enabled.
                    && !(
                        predator.QuirkManager()?.HasSpecialFlag("EnableGoalSwitching") == true 
                        && RV2Mod.Settings.features.IgnoreDesignationsGoalSwitching
                        )
                    )
                {
                    reason = "RV2_EADD_Text_PathInvalidConflictingDesignation".Translate();
                    __result = false;
                    return;
                }
                
                if (!isForAuto && RV2_EsegnAdditions_Settings.eadd.AllowConflictingManualInteractions) return;
                if (predator.QuirkManager()?.HasSpecialFlag("EnableGoalSwitching") == true
                    && RV2_EsegnAdditions_Settings.eadd.AllowGoalSwitchersToProposeConflicting) return;
                
                reason = "RV2_EADD_Text_PathInvalidConflicting".Translate();
                __result = false;
            }
        }
    }
}