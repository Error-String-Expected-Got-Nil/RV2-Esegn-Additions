using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;
using Verse;

namespace RV2_Esegn_Additions
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
            if (!RV2_EsegnAdditions_Settings.eadd.EnableVorePathConflicts) return;
            
            if (ConflictingPathUtils.PathConflictsWithAnyActiveVore(predator, __instance, out _))
            {
                if (!isForAuto && RV2_EsegnAdditions_Settings.eadd.AllowConflictingManualInteractions) return;
                if (predator.QuirkManager()?.HasSpecialFlag("EnableGoalSwitching") == true
                    && RV2_EsegnAdditions_Settings.eadd.AllowGoalSwitchersToProposeConflicting) return; 
                
                reason = "RV2_EADD_Text_PathInvalidConflicting".Translate();
                __result = false;
            }
        }
    }
}