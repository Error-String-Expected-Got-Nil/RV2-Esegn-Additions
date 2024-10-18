using System.Collections.Generic;
using System.Linq;
using RimVore2;
using RimWorld;
using Verse;

#if v1_4
using static RV2_Esegn_Additions.Utilities.CompatibilityUtils;
#endif

namespace RV2_Esegn_Additions;

public class AccidentalDigestionTracker : IExposable
{
    public List<AccidentalDigestionRecord> Records = new List<AccidentalDigestionRecord>();

    public Pawn Predator;
    public bool IsEmpty => Records.Empty();
    public uint Cooldown;

    public float PredatorControlModifier;
    public float PredatorAwarenessModifier;

    public AccidentalDigestionTracker() {}
        
    public AccidentalDigestionTracker(Pawn predator)
    {
        Predator = predator;
        UpdateModifierCache();
    }

    public void TickRare()
    {
        // .ToList() to clone in case a record gets removed during its rare tick
        Records.ToList().ForEach(record => record.TickRare());
            
        if (CanBeginAccidentalDigestion() && RollForAccidentalDigestion()) BeginAccidentalDigestion();
    }

    public void NotifyFinished(AccidentalDigestionRecord record)
    {
        Records.Remove(record);
    }
        
    public bool CanBeginAccidentalDigestion()
    {
        if (!RV2_EADD_Settings.eadd.EnableAccidentalDigestion) return false;
        if (Cooldown > 0) return false;
        if (Predator.PawnData().VoreTracker.VoreTrackerRecords.Empty()) return false;

        if (RV2_EADD_Settings.eadd.CanAlwaysAccidentallyDigest) return true;
        if (!Predator.Awake()) return true;
        if (Predator.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) <= 0.9f) return true;

        return false;
    }
        
    public bool RollForAccidentalDigestion()
    {
        var chance = RV2_EADD_Settings.eadd.BaseAccidentalDigestionTickChance;
        chance *= PredatorControlModifier;

        return RandomUtility.GetRandomFloat() < chance;
    }
        
    // The way target selection works, a given "part" (jumpKey) having multiple prey will increase the likelihood
    // it is selected for accidental digestion.
    //
    // If it turns out the chosen target doesn't have any possible fatal paths they can be jumped to, or the chosen
    // group of targets if path conflicts are enabled, they will be removed from the potential targets and it will
    // try to select a new target.
    public void BeginAccidentalDigestion()
    {
        var potentialTargets = Predator.PawnData().VoreTracker.VoreTrackerRecords
            .Where(record => !record.VoreGoal.IsLethal && record.CurrentVoreStage.def.jumpKey != null)
            .ToList();

        do {
            if (potentialTargets.Empty()) return;

            var initialTarget = potentialTargets.RandomElement();
            List<VoreTrackerRecord> tentativeTargets;
            if (RV2_EADD_Settings.eadd.EnableVorePathConflicts)
                tentativeTargets = potentialTargets.Where(record => 
                        record.CurrentVoreStage.def.jumpKey == initialTarget.CurrentVoreStage.def.jumpKey)
                    .ToList();
            else
                tentativeTargets = new List<VoreTrackerRecord> { initialTarget };

            if (RV2_EADD_Settings.eadd.LongTermPreventsAccidentalDigestion
                && tentativeTargets.Any(record => record.CurrentVoreStage.def.passConditions
                    .Any(condition => condition is StagePassCondition_Manual)))
            {
                potentialTargets.RemoveAll(record => tentativeTargets.Contains(record));
                continue;
            }
                
            // Notable edge case: If a prey is being swallowed and accidental digestion starts in the part they are
            // travelling to, the accidental digestion won't have accounted for them. It will *try* to resolve the
            // path conflict if possible, but it may give up and leave it conflicting if the new prey can't do it.
            Patch_VorePathDef.DisablePathConflictChecks = true;
            var potentialPaths = RV2_Common.VorePaths
                .Where(path => 
                    path.voreGoal.IsLethal
                    && path.stages.Any(stage => stage.jumpKey == initialTarget.CurrentVoreStage.def.jumpKey)
                    && tentativeTargets
                        .Select(record => record.Prey)
                        .All(prey => path.IsValid(Predator, prey, out _, true,
                            RV2_EADD_Settings.eadd.AccidentalDigestionIgnoresDesignations)))
                .ToList();
            Patch_VorePathDef.DisablePathConflictChecks = false;

            if (!potentialPaths.Empty())
            {
                Records.Add(new AccidentalDigestionRecord(tentativeTargets, potentialPaths, this, 
                    initialTarget.CurrentVoreStage.def.jumpKey));
                BeginCooldown();
                return;
            }

            potentialTargets.RemoveAll(record => tentativeTargets.Contains(record));
        } 
        while (true);
    }
        
    public void UpdateModifierCache()
    {
        PredatorControlModifier = Predator.QuirkManager()?
            .TryGetValueModifier("AccidentalDigestionControl",
                ModifierOperation.Multiply, out var predControlMod) == true ? predControlMod : 1f;
            
        PredatorAwarenessModifier = Predator.QuirkManager()?
            .TryGetValueModifier("AccidentalDigestionAwareness",
                ModifierOperation.Multiply, out var predAwarenessMod) == true ? predAwarenessMod : 1f;
    }
        
    public void BeginCooldown()
    {
        Cooldown = RV2_EADD_Settings.eadd.AccidentalDigestionCooldown;
    }

    public void TickCooldown()
    {
        if (Cooldown > 0) Cooldown--;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref Records, nameof(Records), LookMode.Deep);
        Scribe_References.Look(ref Predator, nameof(Predator));
        Scribe_Values.Look(ref Cooldown, nameof(Cooldown));

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            UpdateModifierCache();
        }
    }
}