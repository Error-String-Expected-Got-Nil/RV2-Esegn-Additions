using System.Collections.Generic;
using System.Linq;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2_Esegn_Additions.Utilities
{
    public static class ConflictingPathUtils
    {
        public static bool PathConflictsWithRecord(VoreTrackerRecord record, VorePathDef path)
        { 
            // Probably unnecessary, but can't hurt.
            if (record.IsFinished || record.IsInterrupted) return false;
            
            // Endo never conflicts with endo.
            if (!record.VorePath.def.voreGoal.IsLethal && !path.voreGoal.IsLethal)
            {
                return false;
            }

            // Checks if the record's current stage or any of its future stages have the same jumpKey as any stages in
            // the given path. jumpKey is the only thing the stage defs really provide for knowing if two stages
            // happen in the same "place".
            var pathJumpKeys = path.stages.Select(stage => stage.jumpKey).ToList();

            var checking = false;
            foreach (var stage in record.VorePath.def.stages)
            {
                if (!checking && stage == record.CurrentVoreStage.def) checking = true;
                if (!checking) continue;
                if (stage.jumpKey == null || !pathJumpKeys.Contains(stage.jumpKey)) continue;
                
                // If both are lethal, they can intersect if they have the same vore goal.
                if (record.VorePath.def.voreGoal.IsLethal && path.voreGoal.IsLethal)
                    return record.VorePath.def.voreGoal != path.voreGoal;
                    
                // Otherwise, the paths intersect, one is endo, one is fatal, so they conflict.
                return true;
            }

            return false;
        }
        
        // outRecord is the record that conflicts, it's null if there isn't one
        public static bool PathConflictsWithAnyActiveVore(Pawn predator, VorePathDef path, out VoreTrackerRecord 
            outRecord)
        {
            foreach (var record in predator.PawnData().VoreTracker.VoreTrackerRecords)
            {
                if (!PathConflictsWithRecord(record, path)) continue;
                outRecord = record;
                return true;
            }

            outRecord = null;
            return false;
        }

        // Checks if a record conflicts with any others on the predator and performs a vore jump if possible to fix it
        public static void CheckAndResolvePathConflicts(VoreTrackerRecord record)
        {
            if (PathConflictsWithAnyActiveVore(record.Predator, record.VorePath.def, out var conflictingRecord))
            {
                ResolvePathConflict(record, conflictingRecord);
            }
        }

        // Checks all records for a pred other than the given one, and resolves conflicts with the given record as the
        // conflicting record, so the one who's path gets switched to.
        public static void CheckAndResolveOtherRecords(VoreTrackerRecord givenRecord)
        {
            var recordsToResolve = new List<VoreTrackerRecord>();
            
            foreach (var record in givenRecord.Predator.PawnData().VoreTracker.VoreTrackerRecords)
            {
                if (record == givenRecord) continue;
                
                if (PathConflictsWithRecord(record, givenRecord.VorePath.def)) 
                    recordsToResolve.Add(record);
            }

            foreach (var record in recordsToResolve)
            {
                ResolvePathConflict(record, givenRecord);
            }
        }
        
        private static void ResolvePathConflict(VoreTrackerRecord record, VoreTrackerRecord conflictingRecord)
        {
            var currentJumpKey = record.CurrentVoreStage.def.jumpKey;
            if (currentJumpKey == null) return;
            var targetStage =
                conflictingRecord.VorePath.def.stages.FirstOrFallback(stage => stage.jumpKey == currentJumpKey);
            if (targetStage == null) return;
            
            var targetPath = conflictingRecord.VorePath.def;

            Patch_VorePathDef.DisablePathConflictChecks = true;
            var targetPathIsValid = targetPath.IsValid(record.Predator, record.Prey, out _, true,
                RV2_EADD_Settings.eadd.PathConflictsIgnoreDesignations);
            Patch_VorePathDef.DisablePathConflictChecks = false;
            
            if (!targetPathIsValid) return;
            
            var jump = new VoreJump(targetPath, targetStage);
            Patch_JumpUtility.SkipNextPathJumpNotification = true;
            jump.Jump(record, true);
            
            string message = "RV2_EADD_Text_ConflictingPathResolutionSwitch".Translate(
                record.Predator.Named("PREDATOR"),
                record.Prey.Named("PREY"),
                record.VoreGoal.LabelCap.Named("OLDGOAL"),
                conflictingRecord.VoreGoal.LabelCap.Named("NEWGOAL")
            );
            NotificationUtility.DoNotification(RV2Mod.Settings.fineTuning.GoalSwitchNotification, message, 
                "RV2_EADD_Text_ConflictingPathResolutionSwitch_Key".Translate());
            
            // Have to manually do interactions and add prey memories since for some reason that isn't handled
            // by JumpUtility
            
            var rulePacks = new List<RulePackDef>();
            var typeRules = targetPath.voreType?.relatedRulePacks;
            var goalRules = targetPath.voreGoal?.relatedRulePacks;
            if(typeRules != null)
                rulePacks.AddRange(typeRules);
            if(goalRules != null)
                rulePacks.AddRange(goalRules);
            Find.PlayLog.Add(new PlayLogEntry_Interaction(VoreInteractionDefOf.RV2_SwitchedGoal, 
                record.Predator, record.Prey, rulePacks));
            
            var memories = record.Prey.needs?.mood?.thoughts?.memories;
            if(memories == null)
            {
                Log.Error("Memories of the prey were null");
                return;
            }
            
            memories.TryGainMemory(VoreThoughtDefOf.RV2_SwitchedGoalOnMe_Social, record.Predator);
        }
    }
}