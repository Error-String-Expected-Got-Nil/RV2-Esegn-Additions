using System;
using System.Linq;
using RimVore2;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RV2_Esegn_Additions.Utilities;

public static class EndoanalepticsUtils
{
    private static readonly VoreTargetSelectorRequest HealVoreSelector = new()
    {
        voreGoal = VoreGoalDefOf.Heal, 
        allMustMatch = true
    };
    
    public static void AddTend(Pawn pawn, float tendQuality)
    {
        var hediff = (Hediff_EndoanalepticSupplements)HediffMaker.MakeHediff(
            RV2_EADD_Common.EaddHediffDefOf.RV2_EADD_EndoanalepticSupplementsHediff, pawn);
        hediff.TendQualities.Add(tendQuality);
        pawn.health.AddHediff(hediff);
    }

    public static Hediff_EndoanalepticSupplements GetEndoanaleptics(Pawn pawn)
    {
        return (Hediff_EndoanalepticSupplements)pawn.health.hediffSet.hediffs.Find(hediff => 
            hediff.def == RV2_EADD_Common.EaddHediffDefOf.RV2_EADD_EndoanalepticSupplementsHediff);
    }

    public static bool CanDoHealVore(Pawn predator)
    {
        if (!RV2Mod.Settings.features.EndoVoreEnabled) return false;
        if (!predator.CanBePredator(out _)) return false;

        if (!RV2Mod.Settings.features.VoreQuirksEnabled) return true;
        
        var quirks = predator.QuirkManager(false);
        if (quirks == null) return false;
        if (quirks.HasSpecialFlag("FatalPredatorOnly")) return false;
        if (!quirks.HasVoreEnabler(HealVoreSelector)) return false;

        return true;
    }

    // Get the number of medicine units needed to provide enough endoanaleptics supplements charges to the predator so
    // that all untended hediffs on target can be tended. If predator and target are the same pawn, will instead
    // return based on the number of untended hediffs for all the predator's heal vore prey. 
    public static int GetMedicineNeededForTending(Pawn predator, Pawn target)
    {
        var hediffeas = GetEndoanaleptics(predator);
        var currentCharges = hediffeas == null ? 0 : hediffeas.TendQualities.Count;
        
        if (predator == target)
            return Mathf.Max(0, predator.PawnData().VoreTracker.VoreTrackerRecords
                                    .Where(record => record.VoreGoal == VoreGoalDefOf.Heal)
                                    .Sum(record => record.Prey.health.hediffSet.GetHediffsTendable().Count())
                                - currentCharges);

        return Mathf.Max(0, target.health.hediffSet.GetHediffsTendable().Count() - currentCharges);
    }

    // Variant of HealthAIUtility that doesn't care about the number of wounds on the patient.
    public static Thing FindBestMedicine(Pawn doctor, Pawn patient)
    {
        if (patient.playerSettings is { medCare: <= MedicalCareCategory.NoMeds })
            return null;

        var bestMedOnDoctor = GetBestMedInInventory(doctor.inventory.innerContainer);
        var nearestValidMed = GenClosest.ClosestThing_Global_Reachable(patient.PositionHeld, patient.MapHeld,
            patient.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch,
            TraverseParms.For(doctor), validator: Validator, priorityGetter: PriorityOf);

        // Basic check, find the best medicine on the ground or in doctor's inventory
        if (bestMedOnDoctor != null && nearestValidMed != null)
            return PriorityOf(bestMedOnDoctor) < PriorityOf(nearestValidMed)
                ? nearestValidMed
                : bestMedOnDoctor;

        // Otherwise, check colony animals to see if any are carrying medicine (for caravans, presumably)
        if (bestMedOnDoctor == null && nearestValidMed == null && doctor.IsColonist && doctor.Map != null)
            foreach (var animal in doctor.Map.mapPawns.SpawnedColonyAnimals)
            {
                var bestMedOnAnimal = GetBestMedInInventory(animal.inventory.innerContainer);
                if (bestMedOnAnimal != null
                    && (nearestValidMed == null || PriorityOf(nearestValidMed) < PriorityOf(bestMedOnAnimal))
                    && !animal.IsForbidden(doctor)
                    && doctor.CanReach(animal, PathEndMode.OnCell, Danger.Some))
                    nearestValidMed = bestMedOnAnimal;
            }

        return bestMedOnDoctor ?? nearestValidMed;

        bool Validator(Thing med) 
            => !med.IsForbidden(doctor) 
               && (patient.playerSettings?.medCare ?? MedicalCareCategory.NoMeds).AllowsMedicine(med.def) 
               && doctor.CanReserve(med, 10, 1);
        
        Thing GetBestMedInInventory(ThingOwner inventory) 
            => inventory.Count == 0 
                ? null 
                : inventory.Where(thing => thing.def.IsMedicine && Validator(thing))
                    .OrderByDescending(PriorityOf)
                    .FirstOrDefault();

        float PriorityOf(Thing thing) 
            => thing.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
    }
}