using System.Collections.Generic;
using RimWorld;
using RV2_Esegn_Additions.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RV2_Esegn_Additions;

public class JobDriver_AdministerEndoanaleptics : JobDriver
{
    public const int BaseAdminsterDuration = 300;
    public const int MedicineGrabTime = 25;
    public const float BaseExperienceGain = 150.0f;

    private const TargetIndex PatientIndex = TargetIndex.A;
    private const TargetIndex MedicineIndex = TargetIndex.B;
    private const TargetIndex MedicineHolderIndex = TargetIndex.C;
    
    private PathEndMode pathEndMode;

    private Pawn Patient => job.targetA.Pawn;
    private Thing Medicine => job.targetB.Thing;
    private Pawn MedicineHolderPawn => job.targetC.Pawn;
    // Slightly weird: I *would* put this in one of the target indicies, but there's only 3, and we need the others
    // where they are. So, instead, the reference pawn has to go in the front of the first target queue.
    // Reference pawn is checked for tendable hediffs, and enough medicine is administered to the patient to tend them.
    // If the patient *is* the reference pawn, it checks the patient's heal vore prey for tendable hediffs.
    private Pawn ReferencePawn => job.targetQueueA[0].Pawn;

    public override void Notify_Starting()
    {
        base.Notify_Starting();

        if (Patient == pawn) pathEndMode = PathEndMode.OnCell;
        else if (Patient.InBed()) pathEndMode = PathEndMode.InteractionCell;
        else pathEndMode = PathEndMode.ClosestTouch;
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (Patient != pawn && !pawn.Reserve(Patient, job, errorOnFailed: errorOnFailed)) return false;

        // Check for required medicine expects targetQueueA to exist and have at least one entry, ensure that here.
        job.targetQueueA ??= [];
        if (job.targetQueueA.Count < 1) job.targetQueueA.Add(LocalTargetInfo.Invalid);
        
        var requiredMedicine = GetRequiredMedicineCount();
        if (requiredMedicine < 1) return false;

        // If no medicine was provided before the job was started, automatically find some
        if (Medicine == null) job.targetB = EndoanalepticsUtils.FindBestMedicine(pawn, Patient);
        // If we still don't have any, the job can't start
        if (Medicine == null) return false;
        
        if (pawn.Map.reservationManager.CanReserveStack(pawn, Medicine, 10) <= 0 
            || !pawn.Reserve(Medicine, job, 10, requiredMedicine, errorOnFailed: errorOnFailed)) 
            return false;

        return true;
    }

    // Implementation largely copied from JobDriver_TendPatient
    protected override IEnumerable<Toil> MakeNewToils()
    {
        // Basic checks
        this.FailOnDespawnedNullOrForbidden(PatientIndex);
        this.FailOn(() => pawn.Faction == Faction.OfPlayer &&
                          (!Patient.playerSettings?.medCare.AllowsMedicine(Medicine.def) ?? false)
                          || 
                          pawn == Patient && pawn.Faction == Faction.OfPlayer &&
                          (!pawn.playerSettings?.selfTend ?? false));
        this.FailOnAggroMentalState(PatientIndex);
        AddEndCondition(() => GetRequiredMedicineCount() > 0 ? JobCondition.Ongoing : JobCondition.Succeeded);

        // Pre-declare toils that may be jumped to
        var gotoPatient = Toils_Goto.GotoThing(PatientIndex, pathEndMode);
        var gotoMedicineHolder = Toils_Goto.GotoThing(MedicineHolderIndex, PathEndMode.Touch)
            .FailOn(() => MedicineHolderPawn != (Medicine?.ParentHolder as Pawn_InventoryTracker)?.pawn
                          || MedicineHolderPawn.IsForbidden(pawn));
        
        var reserveMedicine = ToilMaker.MakeToil();
        reserveMedicine.initAction = () =>
        {
            var requiredMedicine = GetRequiredMedicineCount();
            if (requiredMedicine < 1)
            {
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                return;
            }
            
            var reservable = pawn.Map.reservationManager.CanReserveStack(pawn, Medicine, 10);
            if (reservable > 0 && pawn.Reserve(Medicine, job, 10, requiredMedicine)) return;
            pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
        };

        var administerTime = (int)(1.0f / pawn.GetStatValue(StatDefOf.MedicalTendSpeed) * BaseAdminsterDuration);
        Toil administerTimer;

        if (pawn == Patient)
        {
            administerTimer = Toils_General.Wait(administerTime);
        }
        else
        {
#if v1_5
            administerTimer = Toils_General.WaitWith_NewTemp(PatientIndex, administerTime,
                maintainPosture: true, face: PatientIndex, pathEndMode: pathEndMode);
#endif
#if v1_4
            administerTimer = Toils_General.WaitWith(PatientIndex, administerTime, maintainPosture: true,
                face: PatientIndex);
#endif
            administerTimer.AddFinishAction(() =>
            {
                if (Patient != null 
                    && pawn != Patient 
                    && Patient.CurJob != null 
                    && (Patient.CurJob.def == JobDefOf.Wait || Patient.CurJob.def == JobDefOf.Wait_MaintainPosture))
                    Patient.jobs.EndCurrentJob(JobCondition.InterruptForced);
            });
        }
        
        administerTimer.WithProgressBarToilDelay(PatientIndex).PlaySustainerOrSound(SoundDefOf.Interact_Tend);
        administerTimer.activeSkill = () => SkillDefOf.Medicine;
        administerTimer.handlingFacing = true;
        administerTimer.tickAction = () =>
        {
            if (pawn != Patient) pawn.rotationTracker.FaceTarget(Patient);
        };
        administerTimer.FailOn(() =>
            pawn != Patient && !pawn.CanReachImmediate(Patient.SpawnedParentOrMe, pathEndMode));

        // Medicine collecting:
        yield return Toils_Jump.JumpIf(gotoPatient, () => 
            Medicine != null && pawn.inventory.Contains(Medicine));
        yield return Toils_Haul.CheckItemCarriedByOtherPawn(Medicine, MedicineHolderIndex,
            gotoMedicineHolder);
        
        // Medicine was available loosely, reserve the item and go grab it
        yield return reserveMedicine;
        yield return Toils_Goto.GotoThing(MedicineIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(MedicineIndex);
        yield return PickupMedicine().FailOnDestroyedOrNull(MedicineIndex);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveMedicine, MedicineIndex,
            TargetIndex.None, true);
        yield return Toils_Jump.Jump(gotoPatient);
        
        // Chosen medicine was held by another pawn, go and get it from them 
        yield return gotoMedicineHolder;
        yield return Toils_General.Wait(MedicineGrabTime).WithProgressBarToilDelay(MedicineHolderIndex);
        yield return Toils_Haul.TakeFromOtherInventory(Medicine, pawn.inventory.innerContainer,
            (Medicine?.ParentHolder as Pawn_InventoryTracker)?.innerContainer, GetRequiredMedicineCount(),
            MedicineIndex);
        
        // Go to and tend patient
        yield return gotoPatient;
        yield return Toils_Jump.JumpIf(administerTimer, () => 
            Medicine != null && pawn.inventory.Contains(Medicine));
        yield return PickupMedicine().FailOnDestroyedOrNull(MedicineIndex);
        yield return administerTimer;
        yield return ApplyEndoanaleptics();
        yield return GetMoreMedicine(reserveMedicine);
        yield return Toils_Jump.Jump(gotoPatient);
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref pathEndMode, nameof(pathEndMode));
    }

    private int GetRequiredMedicineCount()
    {
        return ReferencePawn != null 
            ? EndoanalepticsUtils.GetMedicineNeededForTending(Patient, ReferencePawn) 
            : job.takeExtraIngestibles;
    }

    private Toil PickupMedicine()
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            var required = GetRequiredMedicineCount();
            if (pawn.carryTracker.CarriedThing != null) required -= pawn.carryTracker.CarriedThing.stackCount;
            
            var count = Mathf.Min(pawn.Map.reservationManager.CanReserveStack(pawn, Medicine, 10), 
                required);
            if (count > 0) pawn.carryTracker.TryStartCarry(Medicine, count);
            
            if (Medicine.Spawned) pawn.Map.reservationManager.Release(Medicine, pawn, job);
            job.SetTarget(MedicineIndex, pawn.carryTracker.CarriedThing);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil ApplyEndoanaleptics()
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            pawn.skills?.Learn(SkillDefOf.Medicine, BaseExperienceGain * Medicine.def.MedicineTendXpGainFactor);

            var baseQuality = TendUtility.CalculateBaseTendQuality(pawn, Patient, Medicine.def);
            var maxQuality = Medicine.def.GetStatValueAbstract(StatDefOf.MedicalQualityMax);
            var quality = Random.Range(baseQuality, maxQuality);
            EndoanalepticsUtils.AddTend(Patient, quality);
            
            Patient.records.Increment(RecordDefOf.TimesTendedTo);
            pawn.records.Increment(RecordDefOf.TimesTendedOther);
            
            // Apparently this is how the game plays the glittertech medicine sound effect
            if ((Patient.Spawned || pawn.Spawned) && Medicine.GetStatValue(StatDefOf.MedicalPotency) 
                > ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency))
                SoundDefOf.TechMedicineUsed.PlayOneShot(new TargetInfo(Patient.Position, Patient.Map));

            if (Medicine.stackCount > 1) Medicine.stackCount--;
            else if (!Medicine.Destroyed) Medicine.Destroy();
            
            if (Medicine?.Destroyed ?? false) job.SetTarget(MedicineIndex, LocalTargetInfo.Invalid);

            // "job.takeExtraIngestibles" is used as a counter for an explicitly provided number of medicine to
            // administer, since it's the closest thing to what I want to use it for, and for some reason the Job class
            // doesn't have ExposeData as virtual, so I can't just extend it.
            if (ReferencePawn == null) job.takeExtraIngestibles--;
            if (GetRequiredMedicineCount() <= 0) pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
        };
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil GetMoreMedicine(Toil reserveMedicine)
    {
        var toil = ToilMaker.MakeToil();
        toil.initAction = () =>
        {
            if (!Medicine.DestroyedOrNull()) return;
            var newMedicine = EndoanalepticsUtils.FindBestMedicine(pawn, Patient);
            if (newMedicine == null) return;
            job.SetTarget(MedicineIndex, newMedicine);
            JumpToToil(reserveMedicine);
        };
        return toil;
    }
}