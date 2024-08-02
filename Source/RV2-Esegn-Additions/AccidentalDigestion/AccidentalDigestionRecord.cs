using System.Collections.Generic;
using System.Linq;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionRecord : IExposable
    {
        public List<RecordPair> Records;

        public List<VoreTrackerRecord> OriginalRecords =>
            (List<VoreTrackerRecord>)Records.Select(record => record.Original); 
        public List<VoreTrackerRecord> SwitchedRecords =>
            (List<VoreTrackerRecord>)Records.Select(record => record.Switched); 
        
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

            var filteredPaths = (List<VorePathDef>)potentialPaths.Where(path => path.voreGoal == VoreGoal);
            
            targets.ForEach(record => MakeSwitchedRecord(record, filteredPaths));
            
            // TODO: Hediff to indicate accidental digestion is happening?
        }

        public void TickRare()
        {
            // Makes checks for anything that would immediately tell the predator they've accidentally digested their
            // prey, mainly cases of digestion finishing for any accidentally digested prey.
            if (Predator.Awake() && SwitchedRecords.Any(CannotRevertRecord))
                ResolveAccidentalDigestion();
            
            if (CanRollAwareness() && RollAwareness()) 
            {
                if (!Predator.Awake()) RestUtility.WakeUp(Predator);
                ResolveAccidentalDigestion();
            }
        }

        private bool CanRollAwareness()
        {
            return RV2_EADD_Settings.eadd.AllowAwarenessRolls;
        }
        
        private bool RollAwareness()
        {
            // If prey must struggle to make the predator aware and none are, this automatically fails.
            if (RV2Mod.Settings.features.StrugglingEnabled
                && RV2_EADD_Settings.eadd.PreyMustStruggleToBeNoticed
                && !SwitchedRecords.Any(record => record.StruggleManager.ShouldStruggle))
                return false;
            
            var chance = RV2_EADD_Settings.eadd.BasePredatorAwarenessChance;
            chance *= Tracker.PredatorAwarenessModifier;

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

        // Immediately resolves this AccidentalDigestionRecord one way or another, either by keeping records as fatal,
        // or by reverting to the original endo paths.
        public void ResolveAccidentalDigestion(bool doNotification = true)
        {
            if (SwitchedRecords.Any(IsDigesting))
            {
                if (doNotification) NotificationUtility.DoNotification(NotificationType.MessageNeutral, 
                    "RV2_EADD_Text_AccidentalDigestionAbortFail".Translate(
                        Predator.Named("PREDATOR")
                        ),
                    "RV2_EADD_Text_AccidentalDigestionAbortFail_Key");
                Tracker.NotifyFinished(this);
                return;
            }
            
            Records.Where(record => !CannotRevertRecord(record.Switched)).ForEach(RevertRecord);
            if (doNotification) NotificationUtility.DoNotification(NotificationType.MessageNeutral, 
                "RV2_EADD_Text_AccidentalDigestionAbort".Translate(
                    Predator.Named("PREDATOR")
                    ),
                "RV2_EADD_Text_AccidentalDigestionAbort_Key");
            Tracker.NotifyFinished(this);
        }

        private static void RevertRecord(RecordPair record)
        {
            var damageDef = AcidUtility.GetDigestDamageDef(record.Switched.CurrentVoreStage.def);
            if (damageDef != null)
            {
                DigestionUtility.ApplyDigestionBookmark(record.Switched);
                AcidUtility.ApplyAcidByDigestionProgress(record.Switched, 
                    record.Switched.CurrentVoreStage.PercentageProgress, damageDef);
            }

            var tracker = record.Switched.VoreTracker;
            tracker.UntrackVore(record.Switched);

            record.Original.CurrentVoreStage.PassedRareTicks = 0;
            tracker.TrackVore(record.Original);
        }

        // TODO: Social memories?
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
            
            Records.Add(new RecordPair { Original = record, Switched = newRecord });
            
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

        // Returns true if the given record cannot be reverted to its original, either because it has finished or
        // because digestion has progressed too far to stop.
        private bool CannotRevertRecord(VoreTrackerRecord record)
        {
            return record.IsFinished 
                   || record.IsInterrupted 
                   || record.CurrentVoreStage.def.jumpKey != JumpKey 
                   || IsDigesting(record);
        }

        // Returns if the record is digesting and is too far into the process to stop.
        private static bool IsDigesting(VoreTrackerRecord record)
        {
            return record.CurrentVoreStage.OnEnd.actions.Any(rollAction => rollAction is RollAction_Digest)
                   && record.CurrentVoreStage.PercentageProgress > RV2_EADD_Settings.eadd.AbortDigestionThreshold;
        }
        
        public void ExposeData()
        {
            Scribe_Collections.Look(ref Records, nameof(Records), LookMode.Deep);
            
            Scribe_References.Look(ref Predator, nameof(Predator));
            Scribe_Values.Look(ref JumpKey, nameof(JumpKey));
            Scribe_Defs.Look(ref VoreGoal, nameof(VoreGoal));
            
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Tracker = AccidentalDigestionManager.Manager.GetTracker(Predator);
            }
        }
    }
}