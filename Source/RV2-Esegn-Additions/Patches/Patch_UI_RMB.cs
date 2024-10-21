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
        if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor)) return;

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
            {
                optionLabel = "RV2_EADD_Text_RMB_AdministerEAS_Self".Translate();
                if (!pawn.playerSettings?.selfTend ?? false)
                {
                    optionLabel += " (" + "RV2_EADD_Text_RMB_AdministerEAS_SelfTendDisabled".Translate() + ")";
                    disabled = true;
                }
            }
            else if (pawn.CanReach(target, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                optionLabel = "RV2_EADD_Text_RMB_AdministerEAS".Translate(target.Pawn.Named("PATIENT"));
            }
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
        ];

        var administerForPreyLabel = "RV2_EADD_Text_RMB_AdministerEAS_ForPrey".Translate();
        if (EndoanalepticsUtils.GetMedicineNeededForTending(target, target) > 0)
        {
            options.Add(new FloatMenuOption(administerForPreyLabel, () =>
            {
                var job = JobMaker.MakeJob(RV2_EADD_Common.EaddJobDefOf.AdministerEndoanaleptics, target, 
                    startingMeds);
                job.targetQueueA = [target];
                doctor.jobs.TryTakeOrderedJob(job);
            }));
        }
        else
        {
            administerForPreyLabel += " (" + (target.PawnData().VoreTracker.VoreTrackerRecords
                                               .Any(record => record.VoreGoal == VoreGoalDefOf.Heal)
                                               ? "RV2_EADD_Text_RMB_AdministerEAS_PreyAlreadyTendable".Translate()
                                               : "RV2_EADD_Text_RMB_AdministerEAS_NoHealPrey".Translate()) 
                                           + ")";
            options.Add(UIUtility.DisabledOption(administerForPreyLabel));
        } 
        
        options.Add(new FloatMenuOption("RV2_EADD_Text_RMB_AdministerEAS_ForTarget".Translate(), () =>
        {
            Find.Targeter.BeginTargeting(EndoanalepticsTargetParams, referencePrey =>
            {
                var job = JobMaker.MakeJob(RV2_EADD_Common.EaddJobDefOf.AdministerEndoanaleptics, target, 
                    startingMeds);
                job.targetQueueA = [referencePrey];
                doctor.jobs.TryTakeOrderedJob(job);
            }, 
                null, referencePrey =>
            {
                if (referencePrey.Pawn == null) return false;

                if (!VoreGoalDefOf.Heal.IsValid(target, referencePrey.Pawn, out var reason)
                    || !target.HasFreeCapacityFor(referencePrey.Pawn))
                {
                    reason ??= "RV2_VoreInvalidReasons_NoCapacity".Translate();
                    
                    NotificationUtility.DoNotification(NotificationType.MessageNeutral, 
                        "RV2_EADD_Text_RMB_AdministerEAS_ReferencePreyCannotBeHealVored"
                            .Translate(target.Named("PREDATOR"), referencePrey.Pawn.Named("PREY"))
                        + " (" + reason + ")");
                    return false;
                }
                
                if (referencePrey.Pawn.health.hediffSet.GetHediffsTendable().EnumerableNullOrEmpty())
                {
                    NotificationUtility.DoNotification(NotificationType.MessageNeutral, 
                        "RV2_EADD_Text_RMB_AdministerEAS_ReferencePreyHasNoTendableHediffs"
                            .Translate(referencePrey.Pawn.Named("PREY")));
                    return false;
                }
                
                if (EndoanalepticsUtils.GetMedicineNeededForTending(target, referencePrey.Pawn) <= 0)
                {
                    NotificationUtility.DoNotification(NotificationType.MessageNeutral, 
                        "RV2_EADD_Text_RMB_AdministerEAS_ReferencePreyAlreadyTendable"
                            .Translate(target.Named("PREDATOR"), referencePrey.Pawn.Named("PREY")));
                    return false;
                }
                
                return true;
            });
        }));

        return options;
    }
}