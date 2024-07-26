using HarmonyLib;
using RimVore2;
using RV2_Esegn_CPI.Utilities;
using Verse;

namespace RV2_Esegn_CPI
{
    [HarmonyPatch(typeof(VorePathDef))]
    public class Patch_VorePathDef
    {
        [HarmonyPatch(nameof(VorePathDef.IsValid))]
        [HarmonyPostfix]
        public static void Postfix_IsValid(Pawn predator, Pawn prey, out string reason, bool isForAuto, 
            bool ignoreDesignations, bool ignoreRules, VorePathDef __instance, ref bool __result)
        {
            reason = null;
            if (!RV2_CPI_Settings.cpi.EnableVorePathConflicts) return;
            
            if (ConflictingPathUtils.PathConflictsWithAnyActiveVore(predator, __instance, out _))
            {
                if (!isForAuto && RV2_CPI_Settings.cpi.AllowConflictingManualInteractions) return;
                if (predator.QuirkManager()?.HasSpecialFlag("EnableGoalSwitching") == true
                    && RV2_CPI_Settings.cpi.AllowGoalSwitchersToProposeConflicting) return; 
                
                reason = "RV2_CPI_Text_PathInvalidConflicting".Translate();
                __result = false;
            }
        }
    }
}