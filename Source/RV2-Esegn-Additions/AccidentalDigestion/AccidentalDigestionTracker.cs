using System.Collections.Generic;
using RimVore2;
using Verse;

#if v1_4
using static RV2_Esegn_Additions.Utilities.CompatibilityUtils;
#endif

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionTracker : IExposable
    {
        public List<AccidentalDigestionRecord> Records = new List<AccidentalDigestionRecord>();

        public bool IsEmpty => Records.Empty();
        public uint Cooldown = 0;
        
        public void UpdateRecord(VoreTrackerRecord record)
        {
            if (record.VoreGoal.IsLethal) return;

            // Necessary in case a record had a path jump and wasn't being tracked by the time this was called.
            if (!record.Predator.PawnData().VoreTracker.VoreTrackerRecords.Contains(record)) return;

            var containingAdr = Records.Find(adr
                => adr.OriginalRecords.Contains(record));
            var jumpKey = record.CurrentVoreStage.def.jumpKey;

            if (containingAdr == null)
            {
                if (jumpKey == null) return;
                AddRecord(record, jumpKey);
                return;
            }
            
            if (jumpKey == containingAdr.JumpKey) return;
            
            containingAdr.RemoveVoreTrackerRecord(record);
            if (containingAdr.OriginalRecords.Empty()) Records.Remove(containingAdr);

            if (jumpKey != null)
            {
                AddRecord(record, jumpKey);
            }
        }

        public void BeginCooldown()
        {
            Cooldown = RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionCooldown;
        }

        public void TickCooldown()
        {
            if (Cooldown > 0) Cooldown--;
        }

        private void AddRecord(VoreTrackerRecord record, string jumpKey)
        {
            if (RV2_EsegnAdditions_Settings.eadd.EnableVorePathConflicts)
            {
                var targetAdr = Records.Find(adr
                    => adr.JumpKey == jumpKey);
                if (targetAdr != null)
                {
                    targetAdr.AddVoreTrackerRecord(record);
                    return;
                }
            }
                    
            Records.Add(new AccidentalDigestionRecord(record));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Records, nameof(Records), LookMode.Deep);
            Scribe_Values.Look(ref Cooldown, nameof(Cooldown));
        }
    }
}