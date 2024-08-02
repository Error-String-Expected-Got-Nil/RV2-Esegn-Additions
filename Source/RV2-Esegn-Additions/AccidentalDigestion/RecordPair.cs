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
            Scribe_References.Look(ref Original, nameof(Original));
            Scribe_References.Look(ref Switched, nameof(Switched));
        }
    }
}