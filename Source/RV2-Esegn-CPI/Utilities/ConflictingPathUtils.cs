using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_CPI.Utilities
{
    public static class ConflictingPathUtils
    {
        public static bool PathConflictsWithRecord(VoreTrackerRecord record, VorePathDef path)
        {
            // Probably unnecessary but can't hurt.
            if (record.IsFinished || record.IsInterrupted) return false;
            
            // Endo vore never conflicts with itself.
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
    }
}