#if v1_5
using LudeonTK;
#endif
using RimVore2;
using Verse;
using RimWorld;

namespace RV2_Esegn_Additions.Utilities
{
    public static class DebugUtils
    {
        [DebugAction("RV2-Esegn", "Print predator records", actionType = DebugActionType.ToolMapForPawns, 
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PrintPredatorRecords(Pawn predator)
        {
            if (predator.PawnData().VoreTracker.VoreTrackerRecords.Count == 0)
            {
                RV2Log.Message("Pawn " + predator.LabelShort + " has no `VoreTrackerRecord`s");
            }
            
            foreach (var record in predator.PawnData().VoreTracker.VoreTrackerRecords)
            {
                RV2Log.Message(record.ToString() 
                               + "|IsFinished: " + record.IsFinished 
                               + "|IsInterrupted: " + record.IsInterrupted);
            }
        }

        [DebugAction("RV2-Esegn", "Begin accidental digestion", 
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void BeginAccidentalDigestion(Pawn predator)
        {
            AccidentalDigestionManager.Manager.GetTracker(predator, false)?.BeginAccidentalDigestion();
        }

        [DebugAction("RV2-Esegn", "Resolve accidental digestion", 
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ResolveAllAccidentalDigestion(Pawn predator)
        {
            AccidentalDigestionManager.Manager.GetTracker(predator, false)?.Records
                .ForEach(record => record.ResolveAccidentalDigestion());
        }
    }
}