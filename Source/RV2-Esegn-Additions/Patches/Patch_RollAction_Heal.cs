using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;
using Verse;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(RollAction_Heal))]
public class Patch_RollAction_Heal
{
    public static VoreTrackerRecord ActiveRecord;

    [HarmonyPatch(nameof(RollAction_Heal.TryAction))]
    [HarmonyPrefix]
    public static void Patch_TryAction_Prefix(VoreTrackerRecord record)
    {
        ActiveRecord = record;
    }
    
    [HarmonyPatch(nameof(RollAction_Heal.TryAction))]
    [HarmonyPostfix]
    public static void Patch_TryAction_Postfix()
    {
        ActiveRecord = null;
    }
    
    [HarmonyPatch("TendPawn")]
    [HarmonyPrefix]
    public static bool Patch_TendPawn(ref float rollStrength)
    {
        var pawn = ActiveRecord.Predator;
        var hediffeas = EndoanalepticsUtils.GetEndoanaleptics(pawn);

        // Little confusing, but how the tooltips are set, heal_wait being true means endoanaleptics should be ignored
        if (pawn.PawnData()?.Designations.TryGetValue(RV2_EADD_Common.EaddDesignationDefOf.heal_wait)?
                .IsEnabled() == false && hediffeas == null)
            return false;

        if (hediffeas != null) rollStrength = hediffeas.PopRandomTend();
        
        return true;
    }
}