using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class RecordPair : IExposable
    {
        public VoreTrackerRecord Original;
        public VoreTrackerRecord Switched;
        
        public void ExposeData()
        {
            // Original VTR is no longer being tracked so it won't be deep-saved, need to do it here instead.
            Scribe_Deep.Look(ref Original, nameof(Original));
            Scribe_References.Look(ref Switched, nameof(Switched));
        }
    }
}