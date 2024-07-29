using System.Collections.Generic;
using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionManager : GameComponent
    {
        public static AccidentalDigestionManager Manager;

        private Dictionary<int, AccidentalDigestionTracker> _trackers =
            new Dictionary<int, AccidentalDigestionTracker>();
        
        // Dead references will get cleaned out when saving and loading, as they won't be saved.
        // Using weak references here is probably overkill... I could just save the IDs of any records where accidental
        // digestion happened, but there's no good way to remove the IDs when the records die, so it's technically a
        // memory leak, even if it will never realistically become a problem. But it displeases me to have it like that
        // anyways, so weak references it is.
        public List<WeakReference<VoreTrackerRecord>> RecordsWhereAccidentalDigestionOccurred 
            = new List<WeakReference<VoreTrackerRecord>>();
        
        public AccidentalDigestionManager(Game game)
        {
            Manager = this;
        }

        public AccidentalDigestionTracker GetTracker(Pawn predator, bool createIfNonexistent = true)
        {
            if (_trackers.TryGetValue(predator.thingIDNumber, out var tracker))
                return tracker;

            if (!createIfNonexistent) return null;
            
            tracker = new AccidentalDigestionTracker(predator);
            _trackers.Add(predator.thingIDNumber, tracker);
            return tracker;
        }
        
        // TODO: Check for and clear out empty AccidentalDigestionTrackers every so often, every long tick?

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _trackers, nameof(_trackers), LookMode.Value, 
                LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars && _trackers == null)
                _trackers = new Dictionary<int, AccidentalDigestionTracker>();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var temp = (List<VoreTrackerRecord>) RecordsWhereAccidentalDigestionOccurred
                    .Where(weakRef => weakRef.IsAlive)
                    .Select(weakRef => weakRef.Target);
                Scribe_Collections.Look(ref temp, nameof(RecordsWhereAccidentalDigestionOccurred), 
                    LookMode.Reference);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                var temp = new List<VoreTrackerRecord>();
                Scribe_Collections.Look(ref temp, nameof(RecordsWhereAccidentalDigestionOccurred), 
                    LookMode.Reference);

                RecordsWhereAccidentalDigestionOccurred = (List<WeakReference<VoreTrackerRecord>>)
                    temp.Select(vtr => new WeakReference<VoreTrackerRecord>(vtr));
            }
        }
    }
}