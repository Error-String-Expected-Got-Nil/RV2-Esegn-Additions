﻿using System.Collections.Generic;
using System.Linq;
using RimVore2;
using RV2_Esegn_Additions.Utilities;
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
        public List<ExposableWeakReference<VoreTrackerRecord>> RecordsWhereAccidentalDigestionOccurred 
            = new List<ExposableWeakReference<VoreTrackerRecord>>();
        
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

            // Remove all dead records before saving
            if (Scribe.mode == LoadSaveMode.Saving)
                RecordsWhereAccidentalDigestionOccurred.RemoveAll(weakRef => !weakRef.IsAlive);
            
            Scribe_Collections.Look(ref RecordsWhereAccidentalDigestionOccurred, 
                nameof(RecordsWhereAccidentalDigestionOccurred), LookMode.Deep);
            
            switch (Scribe.mode)
            {
                case LoadSaveMode.Saving:
                {
                    // Save only trackers that are not empty, or have a remaining cooldown.
                    var temp = _trackers
                        .Where(tracker => 
                            !tracker.Value.IsEmpty 
                            || tracker.Value.Cooldown > 0)
                        .ToDictionary();
                    Scribe_Collections.Look(ref temp, nameof(_trackers), LookMode.Value, 
                        LookMode.Deep);
                    
                    break;
                }
                case LoadSaveMode.LoadingVars:
                {
                    Scribe_Collections.Look(ref _trackers, nameof(_trackers), LookMode.Value, 
                        LookMode.Deep);
                
                    if (_trackers == null) _trackers = new Dictionary<int, AccidentalDigestionTracker>();
                    if (RecordsWhereAccidentalDigestionOccurred == null)
                        RecordsWhereAccidentalDigestionOccurred = new List<ExposableWeakReference<VoreTrackerRecord>>();
                    
                    break;
                }
            }
        }
    }
}