using System.Collections.Generic;
using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionRecord : IExposable
    {
        public List<VoreTrackerRecord> OriginalRecords;
        public List<VoreTrackerRecord> SwitchedRecords = new List<VoreTrackerRecord>();
        
        public AccidentalDigestionTracker Tracker;
        public Pawn Predator;
        public string JumpKey;
        public VoreGoalDef VoreGoal;
        
        public AccidentalDigestionRecord(List<VoreTrackerRecord> targets, List<VorePathDef> potentialPaths, 
            AccidentalDigestionTracker tracker, string jumpKey)
        {
            Tracker = tracker;
            Predator = tracker.Predator;
            JumpKey = jumpKey;
            VoreGoal = potentialPaths.Select(path => path.voreGoal).RandomElement();

            var filteredPaths = (List<VorePathDef>) potentialPaths.Where(path => path.voreGoal == VoreGoal);
            
            OriginalRecords = targets;
            OriginalRecords.ForEach(record => MakeSwitchedRecord(record, filteredPaths));
        }

        private void MakeSwitchedRecord(VoreTrackerRecord record, List<VorePathDef> paths)
        {
            var sameTypePaths = 
                paths.FindAll(path => path.voreType == record.VoreType);
            
            // Prefer paths that are the same type as the original path, if possible.
            var targetPath = sameTypePaths.Empty() ? paths.RandomElement() : sameTypePaths.RandomElement();

            var tracker = record.VoreTracker;
            tracker.UntrackVore(record);

            // Uses a patch to make the next ShouldStruggle() check act as if the vore was forced even if it
            // isn't, so that prey will automatically start struggling when accidental digestion happens.
            Patch_SettingsContainer_Rules.MakeNextShouldStruggleCheckForced = true;
            var newRecord = tracker.SplitOffNewVore(record, record.Prey,
                new VorePath(targetPath),
                targetPath.stages.Find(stage => stage.jumpKey == JumpKey).index, true);
            
            AccidentalDigestionManager.Manager.RecordsWhereAccidentalDigestionOccurred
                .Add(new WeakReference<VoreTrackerRecord>(record));
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
            
            NotificationUtility.DoNotification(RV2_EADD_Settings.eadd.AccidentalDigestionNotificationType,
                "RV2_EADD_Text_AccidentalDigestionNotification".Translate(
                        newRecord.Predator.Named("PREDATOR"),
                        newRecord.Prey.Named("PREY"),
                        newRecord.VoreGoal.Named("GOAL")
                    ), "RV2_EADD_Text_AccidentalDigestionNotification_Key".Translate());
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref OriginalRecords, nameof(OriginalRecords), LookMode.Reference);
            Scribe_Collections.Look(ref SwitchedRecords, nameof(SwitchedRecords), LookMode.Reference);
            
            Scribe_References.Look(ref Predator, nameof(Predator));
            Scribe_Values.Look(ref JumpKey, nameof(JumpKey));
            Scribe_Defs.Look(ref VoreGoal, nameof(VoreGoal));
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Tracker = AccidentalDigestionManager.Manager.GetTracker(Predator, false);
            }
        }
    }
}