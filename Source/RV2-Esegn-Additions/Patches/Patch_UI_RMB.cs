using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimVore2;
using RimWorld;
using RV2_Esegn_Additions.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(FloatMenuMakerMap))]
public class Patch_UI_RMB
{
    private static readonly TargetingParameters EndoanalepticsTargetParams = new()
    {
        canTargetPawns = true,
        canTargetAnimals = true,
        canTargetBuildings = false
    };
    
    // This adds options for administering endoanaleptics to the right click menu
    [HarmonyPatch("ChoicesAtFor")]
    [HarmonyPostfix]
    public static void Patch_ChoicesAt(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> __result)
    {
        if (!RV2_EADD_Settings.eadd.EnableEndoanalepticsSupplements) return;
        if (pawn.jobs == null) return;

        var targets = GenUI.TargetsAt(clickPos, EndoanalepticsTargetParams)
            .Where(target => target.Pawn != null
                             && !target.Pawn.IsBurning()
                             && !target.Pawn.HostileTo(Faction.OfPlayer)
                             && !target.Pawn.InMentalState
                             && EndoanalepticsUtils.CanDoHealVore(target.Pawn))
            .ToList();

        if (targets.NullOrEmpty()) return;

        foreach (var target in targets)
        {
            string optionLabel;
            var disabled = false;
            
            if (target.Pawn == pawn)
                optionLabel = "RV2_EADD_Text_RMB_AdministerEAS_Self".Translate();
            else if (pawn.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly))
                optionLabel = "RV2_EADD_Text_RMB_AdministerEAS".Translate(target.Pawn.Named("PATIENT"));
            else
            {
                optionLabel = "RV2_EADD_Text_RMB_AdministerEAS".Translate(target.Pawn.Named("PATIENT"))
                              + " (" + "RV2_EADD_Text_RMB_AdministerEAS_CannotReach".Translate() + ")";
                disabled = true;
            }
            
            if (disabled)
            {
                __result.Add(UIUtility.DisabledOption(optionLabel));
                continue;
            }

            var meds = EndoanalepticsUtils.FindBestMedicine(pawn, target.Pawn);
            if (meds == null)
            {
                optionLabel += " (" + "RV2_EADD_Text_RMB_AdministerEAS_NoMeds".Translate() + ")";
                __result.Add(UIUtility.DisabledOption(optionLabel));
                continue;
            }

            __result.Add(new FloatMenuOption(optionLabel, () => 
                Find.WindowStack.Add(new FloatMenu(GetJobInitOptions(pawn, target.Pawn, meds)))));
        }
    }

    private static List<FloatMenuOption> GetJobInitOptions(Pawn doctor, Pawn target, Thing startingMeds)
    {
        List<FloatMenuOption> options = 
        [
            new("RV2_EADD_Text_RMB_AdministerEAS_x1".Translate(), () =>
            {
                var job = JobMaker.MakeJob(RV2_EADD_Common.EaddJobDefOf.AdministerEndoanaleptics, target, 
                    startingMeds);
                job.takeExtraIngestibles = 1;
                doctor.jobs.TryTakeOrderedJob(job);
            }),

            new("RV2_EADD_Text_RMB_AdministerEAS_x5".Translate(), () =>
            {
                var job = JobMaker.MakeJob(RV2_EADD_Common.EaddJobDefOf.AdministerEndoanaleptics, target, 
                    startingMeds);
                job.takeExtraIngestibles = 5;
                doctor.jobs.TryTakeOrderedJob(job);
            }),

            new("RV2_EADD_Text_RMB_AdministerEAS_x10".Translate(), () =>
            {
                var job = JobMaker.MakeJob(RV2_EADD_Common.EaddJobDefOf.AdministerEndoanaleptics, target, 
                    startingMeds);
                job.takeExtraIngestibles = 10;
                doctor.jobs.TryTakeOrderedJob(job);
            }),

            new("RV2_EADD_Text_RMB_AdministerEAS_ForPrey".Translate(), () => { }),

            new("RV2_EADD_Text_RMB_AdministerEAS_ForTarget".Translate(), () => { })
        ];

        return options;
    }
}