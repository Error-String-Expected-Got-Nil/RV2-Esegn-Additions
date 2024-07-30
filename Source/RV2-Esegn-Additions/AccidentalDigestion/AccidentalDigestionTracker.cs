using System.Collections.Generic;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionTracker : IExposable
    {
        private List<AccidentalDigestionRecord> _records = new List<AccidentalDigestionRecord>();

        public bool IsEmpty => _records.Empty();
        
        // This function is a gangly mess and I am not proud of it.
        public void UpdateRecord(VoreTrackerRecord record)
        {
            if (record.VoreGoal.IsLethal) return;

            var containingAdr = _records.Find(adr
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
            if (containingAdr.OriginalRecords.Empty()) _records.Remove(containingAdr);

            if (jumpKey != null)
            {
                AddRecord(record, jumpKey);
            }
        }

        private void AddRecord(VoreTrackerRecord record, string jumpKey)
        {
            if (RV2_EsegnAdditions_Settings.eadd.EnableVorePathConflicts)
            {
                var targetAdr = _records.Find(adr
                    => adr.JumpKey == jumpKey);
                if (targetAdr != null)
                {
                    targetAdr.AddVoreTrackerRecord(record);
                    return;
                }
            }
                    
            _records.Add(new AccidentalDigestionRecord(record));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _records, nameof(_records), LookMode.Deep);
        }
    }
}