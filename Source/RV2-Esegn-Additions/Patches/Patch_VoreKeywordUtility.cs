using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch(typeof(VoreKeywordUtility))]
    public class Patch_VoreKeywordUtility
    {
        [HarmonyPatch(nameof(VoreKeywordUtility.RecordKeywords))]
        [HarmonyPostfix]
        public static void Patch_RecordKeywords(VoreTrackerRecord record, List<string> __result)
        {
            // Don't need to select only alive references since the reference must be alive if we got here in the
            // first place.
            if (AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                .Select(weakRef => weakRef.Target)
                .Contains(record))
                __result.AddDistinct("AccidentalDigestionOccurred");
        }

        // VoreKeywordUtility.PawnKeywords() is entirely used for determining quirk validity, for which we don't care
        // about path conflicts, so we disable those while getting pawn keywords.
        [HarmonyPatch(nameof(VoreKeywordUtility.PawnKeywords))]
        [HarmonyPrefix]
        public static void Prefix_PawnKeywords()
        {
            Patch_VorePathDef.DisablePathConflictChecks = true;
        }
        
        [HarmonyPatch(nameof(VoreKeywordUtility.PawnKeywords))]
        [HarmonyPostfix]
        public static void Postfix_PawnKeywords()
        {
            Patch_VorePathDef.DisablePathConflictChecks = false;
        }
    }
}