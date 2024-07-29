using System.Collections.Generic;
using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionRecord : IExposable
    {
        public List<VoreTrackerRecord> OriginalRecords = new List<VoreTrackerRecord>();
        public List<VoreTrackerRecord> SwitchedRecords = new List<VoreTrackerRecord>();

        public Pawn Predator;
        public string JumpKey;

        public bool IsAccidentallyDigesting => SwitchedRecords.Count > 0;

        // Assumes the current stage of the initial record has a jump key.
        public AccidentalDigestionRecord(VoreTrackerRecord initial)
        {
            Predator = initial.Predator;
            JumpKey = initial.CurrentVoreStage.def.jumpKey;
            AddVoreTrackerRecord(initial);
        }

        public void RollForAccidentalDigestion()
        {
            
        }

        public void BeginAccidentalDigestion(List<VorePathDef> potentialPaths)
        {
            if (potentialPaths.Empty()) return;

            foreach (var originalRecord in OriginalRecords)
            {
                var sameTypePaths = 
                    potentialPaths.FindAll(path => path.voreType == originalRecord.VoreType);
                
                // Prefer paths that are the same type as the original path, if possible.
                var targetPath = sameTypePaths.Empty() ? sameTypePaths.RandomElement() 
                    : potentialPaths.RandomElement();

                var tracker = originalRecord.VoreTracker;
                tracker.UntrackVore(originalRecord);
                var newRecord = tracker.SplitOffNewVore(originalRecord, originalRecord.Prey,
                    new VorePath(targetPath),
                    targetPath.stages.Find(stage => stage.jumpKey == JumpKey).index, true);
                
                AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                    .Add(new WeakReference<VoreTrackerRecord>(originalRecord));
                AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                    .Add(new WeakReference<VoreTrackerRecord>(newRecord));

                // Switched records should always have the same index as their original, and vice-versa.
                SwitchedRecords.Add(newRecord);
            }
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
        public List<VorePathDef> PotentialTargetPaths()
        {
            Patch_VorePathDef.DisablePathConflictChecks = true;
            var ret = RV2_Common.VorePaths.FindAll(path =>
                path.voreGoal.IsLethal 
                && path.stages.Any(stage => stage.jumpKey == JumpKey)
                && OriginalRecords.All(record => record.VorePath.def.IsValid(Predator, record.Prey, out _, true, 
                    RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionIgnoresDesignations)
                )
            );
            Patch_VorePathDef.DisablePathConflictChecks = false;
            return ret;
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref OriginalRecords, nameof(OriginalRecords), LookMode.Reference);
            Scribe_Collections.Look(ref SwitchedRecords, nameof(SwitchedRecords), LookMode.Reference);
            
            Scribe_References.Look(ref Predator, nameof(Predator), true);
            Scribe_Values.Look(ref JumpKey, nameof(JumpKey));
        }
    }
}