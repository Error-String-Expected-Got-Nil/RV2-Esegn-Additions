using System.Collections.Generic;
using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionManager : GameComponent
    {
        public static AccidentalDigestionManager Manager;

        // Trackers with no records are not included when the game is saved.
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

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                // Save only alive references, and save them as direct references. 
                var temp = (List<VoreTrackerRecord>) RecordsWhereAccidentalDigestionOccurred
                    .Where(weakRef => weakRef.IsAlive)
                    .Select(weakRef => weakRef.Target);
                Scribe_Collections.Look(ref temp, nameof(RecordsWhereAccidentalDigestionOccurred), 
                    LookMode.Reference);

                // Save only trackers that are not empty, or have a remaining cooldown.
                var temp2 = (Dictionary<int, AccidentalDigestionTracker>) _trackers
                    .Where(tracker => 
                        !tracker.Value.IsEmpty || tracker.Value.Cooldown > 0);
                Scribe_Collections.Look(ref temp2, nameof(_trackers), LookMode.Value, 
                    LookMode.Deep);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                // Load as direct references...
                var temp = new List<VoreTrackerRecord>();
                Scribe_Collections.Look(ref temp, nameof(RecordsWhereAccidentalDigestionOccurred), 
                    LookMode.Reference);

                // ...then make them weak references when putting them into the actual list.
                RecordsWhereAccidentalDigestionOccurred = (List<WeakReference<VoreTrackerRecord>>)
                    temp.Select(vtr => new WeakReference<VoreTrackerRecord>(vtr));

                if (_trackers == null) _trackers = new Dictionary<int, AccidentalDigestionTracker>();
                Scribe_Collections.Look(ref _trackers, nameof(_trackers), LookMode.Value, 
                    LookMode.Deep);
            }
        }
    }
}