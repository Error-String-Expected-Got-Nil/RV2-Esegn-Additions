using System;
using System.Linq;
using HarmonyLib;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(ThoughtWorker_Situation_Vore))]
public class Patch_ThoughtWorker_Situation_Vore
{
    // Replaces the normal situational thought handler for vore thoughts, since the modifications I need to do
    // would be a headache with a transpiler, and it's simple enough to just copy over here entirely.
    //
    // For the purposes of situational thoughts, makes the predator think they still have their original records.
    [HarmonyPatch("CurrentStateInternal")]
    [HarmonyPrefix]
    public static bool Patch_ThoughtStateInternal(Pawn predator, ThoughtWorker __instance, 
        ref ThoughtState __result)
    {
        if (!predator.IsActivePredator())
        {
            __result = false;
            return false;
        }

        var records = predator.PawnData()?.VoreTracker?.VoreTrackerRecords.ToList();
        if (records == null)
        {
            __result = ThoughtState.Inactive;
            return false;
        }

        // Replace any switched records with their originals
        var adtracker = AccidentalDigestionManager.Manager
            .GetTracker(predator, false);
        if (adtracker != null)
        {
            adtracker.Records.ForEach(adrecord => 
                adrecord.SwitchedRecords.ForEach(record => records.Remove(record)));
            adtracker.Records.ForEach(adrecord => 
                adrecord.OriginalRecords.ForEach(record => records.Add(record)));
        }

        records = records.FindAll(record => record.CurrentVoreStage.def.CurrentThought(predator) == __instance.def);

        var numPrey = records.Count;
        if (numPrey == 0)
        {
            __result = ThoughtState.Inactive;
            return false;
        }

        var stagesCount = __instance.def.stages.Count - 1;
        var stagesToSet = Math.Min(numPrey - 1, stagesCount);
        __result = ThoughtState.ActiveAtStage(stagesToSet);
            
        return false;
    }
}