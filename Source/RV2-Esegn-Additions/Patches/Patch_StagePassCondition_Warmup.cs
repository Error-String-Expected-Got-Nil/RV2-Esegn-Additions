using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(StagePassCondition_Warmup))]
public class Patch_StagePassCondition_Warmup
{
    [HarmonyPatch(nameof(StagePassCondition_Warmup.IsPassed))]
    [HarmonyPostfix]
    public static void Patch_IsPassed(VoreTrackerRecord record, ref bool __result)
    {
        if (!RV2_EADD_Settings.eadd.EndoanalepticsSkipWarmup) return;
        if (record.VoreGoal != VoreGoalDefOf.Heal) return;
        if (__result) return;

        __result = EndoanalepticsUtils.GetEndoanaleptics(record.Predator) != null;
    }
}