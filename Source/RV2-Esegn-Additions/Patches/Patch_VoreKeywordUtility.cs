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
    }
}