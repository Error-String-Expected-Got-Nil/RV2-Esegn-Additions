﻿using System.Collections.Generic;
using System.Linq;
using RimVore2;
using RimWorld;
using Verse;

#if v1_4
using static RV2_Esegn_Additions.Utilities.CompatibilityUtils;
#endif

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionRecord_DEPRECATED : IExposable
    {
        public List<VoreTrackerRecord> OriginalRecords = new List<VoreTrackerRecord>();
        public List<VoreTrackerRecord> SwitchedRecords = new List<VoreTrackerRecord>();

        public Pawn Predator;
        public string JumpKey;
        public AccidentalDigestionTracker Tracker;

        private float _predatorAwarenessModifier = 1f;
        private float _predatorControlModifier = 1f;
        private List<VorePathDef> _potentialPaths;

        public bool IsAccidentallyDigesting { get; private set; } = false;
        public bool PredatorIsAware { get; private set; } = false;
        public VoreGoalDef VoreGoal { get; private set; } = null;

        // Assumes the current stage of the initial record has a jump key.
        public AccidentalDigestionRecord_DEPRECATED(VoreTrackerRecord initial)
        {
            Predator = initial.Predator;
            JumpKey = initial.CurrentVoreStage.def.jumpKey;
            UpdateModifierCache();
            _potentialPaths = PotentialTargetPaths(CurrentAndPotentialPrey());
            Tracker = AccidentalDigestionManager.Manager.GetTracker(Predator, false);
            AddVoreTrackerRecord(initial);
        }

        public bool CanBeginAccidentalDigestion()
        {
            if (!RV2_EsegnAdditions_Settings.eadd.EnableAccidentalDigestion) return false;
            if (Tracker.Cooldown > 0) return false;
            if (_potentialPaths.Empty()) return false;

            if (RV2_EsegnAdditions_Settings.eadd.LongTermPreventsAccidentalDigestion
                && OriginalRecords.Any(record => record.CurrentVoreStage.def.passConditions
                    .Any(condition => condition is StagePassCondition_Manual)))
                return false;

            if (RV2_EsegnAdditions_Settings.eadd.CanAlwaysAccidentallyDigest) return true;
            if (Predator.needs.rest.Resting) return true;
            if (Predator.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) <= 0.9f) return true;

            return false;
        }

        public bool RollForAccidentalDigestion()
        {
            var chance = RV2_EsegnAdditions_Settings.eadd.BaseAccidentalDigestionTickChance;
            chance *= _predatorControlModifier;

            return RandomUtility.GetRandomFloat() < chance;
        }
        
        public bool RollPredatorAwareness()
        {
            // If prey must struggle to make the predator aware and none are, this automatically fails.
            if (RV2Mod.Settings.features.StrugglingEnabled
                && RV2_EsegnAdditions_Settings.eadd.PreyMustStruggleToBeNoticed
                && !SwitchedRecords.Any(record => record.StruggleManager.ShouldStruggle))
                return false;
            
            var chance = RV2_EsegnAdditions_Settings.eadd.BasePredatorAwarenessChance;
            chance *= _predatorAwarenessModifier;

            var highestStruggle = 0f;
            if (RV2Mod.Settings.features.StrugglingEnabled) 
                highestStruggle = SwitchedRecords
                    .Select(record => record.StruggleManager.StruggleProgress)
                    .Prepend(highestStruggle)
                    .Max();

            var struggleProgressMod = highestStruggle + 1f;
            chance *= struggleProgressMod;

            return RandomUtility.GetRandomFloat() < chance;
        }
        
        public void BeginAccidentalDigestion(List<VorePathDef> potentialPaths)
        {
            if (potentialPaths.Empty()) return;

            VoreGoal = VoreGoal ?? potentialPaths.Select(path => path.voreGoal).RandomElement();

            foreach (var originalRecord in OriginalRecords)
            {
                MakeSwitchedRecord((List<VorePathDef>) potentialPaths.Where(path => path.voreGoal == VoreGoal),
                    originalRecord);

                // TODO: Social memories?
                // TODO: NOTE: For checking if accidental digestion can be reversed, use the "IncreaseDigestionProgress" RollAction
            }
            
            IsAccidentallyDigesting = true;
            Tracker.BeginCooldown();
        }

        private void MakeSwitchedRecord(List<VorePathDef> potentialPaths, VoreTrackerRecord originalRecord)
        {
            var sameTypePaths = 
                potentialPaths.FindAll(path => path.voreType == originalRecord.VoreType);
            
            // Prefer paths that are the same type as the original path, if possible.
            var targetPath = sameTypePaths.Empty() ? potentialPaths.RandomElement()
                : sameTypePaths.RandomElement();

            var tracker = originalRecord.VoreTracker;
            tracker.UntrackVore(originalRecord);

            // Uses a patch to make the next ShouldStruggle() check act as if the vore was forced even if it
            // isn't, so that prey will automatically start struggling when accidental digestion happens.
            Patch_SettingsContainer_Rules.MakeNextShouldStruggleCheckForced = true;
            var newRecord = tracker.SplitOffNewVore(originalRecord, originalRecord.Prey,
                new VorePath(targetPath),
                targetPath.stages.Find(stage => stage.jumpKey == JumpKey).index, true);
            
            AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                .Add(new WeakReference<VoreTrackerRecord>(originalRecord));
            AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                .Add(new WeakReference<VoreTrackerRecord>(newRecord));

            // Switched records should always have the same index as their original, and vice-versa.
            SwitchedRecords.Add(newRecord);
            
            var rulePacks = new List<RulePackDef>();
            var typeRules = newRecord.VorePath.VoreType?.relatedRulePacks;
            var goalRules = newRecord.VorePath.VoreGoal?.relatedRulePacks;
            if (typeRules != null)
                rulePacks.AddRange(typeRules);
            if (goalRules != null)
                rulePacks.AddRange(goalRules);
            var interaction = new PlayLogEntry_Interaction(
                RV2_EADD_Common.EaddInteractionDefOf.RV2_EADD_AccidentalDigestionInteraction, 
                newRecord.Predator, newRecord.Prey, rulePacks);
            Find.PlayLog.Add(interaction);
            
            NotificationUtility.DoNotification(RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionNotificationType,
                "RV2_EADD_Text_AccidentalDigestionNotification".Translate(
                        newRecord.Predator.Named("PREDATOR"),
                        newRecord.Prey.Named("PREY"),
                        newRecord.VoreGoal.Named("GOAL")
                    ), "RV2_EADD_Text_AccidentalDigestionNotification_Key".Translate());
        }

        // TODO: Update cache when quirk menu is closed
        public void UpdateModifierCache()
        {
            _predatorAwarenessModifier = Predator.QuirkManager()?
                .TryGetValueModifier("AccidentalDigestionAwareness",
                ModifierOperation.Multiply, out var predAwarenessMod) == true ? predAwarenessMod : 1f;

            _predatorControlModifier = Predator.QuirkManager()?
                .TryGetValueModifier("AccidentalDigestionControl",
                ModifierOperation.Multiply, out var predControlMod) == true ? predControlMod : 1f;
        }

        // TODO: Any extra work necessary for adding a record (mainly if accidental digestion is already underway)
        public void AddVoreTrackerRecord(VoreTrackerRecord record)
        {
            OriginalRecords.Add(record);
        }

        public void RemoveVoreTrackerRecord(VoreTrackerRecord record)
        {
            OriginalRecords.Remove(record);
        }
        
        // Temporarily disable path conflicts so we can find paths we can jump to. Also utilizes a minor patch to the
        // vore validator to ignore capacity requirements for already-eaten prey.
        // TODO: Update cached potential paths when RV2 settings are updated
        // TODO: Also update when a predator consumes new prey that will enter this record, accounting for them
        private List<VorePathDef> PotentialTargetPaths(List<Pawn> preythings)
        {
            if (preythings == null) return new List<VorePathDef>();
            
            Patch_VorePathDef.DisablePathConflictChecks = true;
            var ret = (List<VorePathDef>) RV2_Common.VorePaths
                .FindAll(path => 
                    path.voreGoal.IsLethal 
                    && path.stages.Any(stage => stage.jumpKey == JumpKey)
                    && preythings.All(prey => path.IsValid(Predator, prey, out _, true, 
                        RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionIgnoresDesignations)
                    ))
                .Where(path => VoreGoal == null || path.voreGoal == VoreGoal);
            Patch_VorePathDef.DisablePathConflictChecks = false;
            return ret;
        }

        // Returns all prey currently in this AD record, and if path conflicts are enabled, any that may be placed in
        // this AD record at some point in the future.
        // Only valid if accidental digestion is not already occurring.
        private List<Pawn> CurrentAndPotentialPrey()
        {
            // If path conflicts are disabled then we only care about the first record, if any. Returns null if there
            // are no records. Without path conflicts only new AD records can gain VTRs to track.
            if (!RV2_EsegnAdditions_Settings.eadd.EnableVorePathConflicts)
                return OriginalRecords.Empty() ? null : new List<Pawn> { OriginalRecords[0].Prey };

            // This is such a gangly LINQ query. I love it.
            return (List<Pawn>) OriginalRecords
                .Select(record => record.Prey)
                .Concat(Predator.PawnData().VoreTracker.VoreTrackerRecords
                    .Where(record => !OriginalRecords.Contains(record)
                                     && record.VorePath.def.stages
                                         .Where(stage => stage.index >= record.CurrentVoreStage.def.index)
                                         .Any(stage => stage.jumpKey == JumpKey))
                    .Select(record => record.Prey)
                );
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref OriginalRecords, nameof(OriginalRecords), LookMode.Reference);
            Scribe_Collections.Look(ref SwitchedRecords, nameof(SwitchedRecords), LookMode.Reference);
            
            Scribe_References.Look(ref Predator, nameof(Predator), true);
            Scribe_Values.Look(ref JumpKey, nameof(JumpKey));

            if (Scribe.mode == LoadSaveMode.PostLoadInit) 
            {
                UpdateModifierCache();
                _potentialPaths = PotentialTargetPaths(CurrentAndPotentialPrey());
                Tracker = AccidentalDigestionManager.Manager.GetTracker(Predator, false);
            }
        }
    }
}