#if v1_5
using LudeonTK;
#endif
using System.Linq;
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
        
        // Assumes only one prey
        [DebugAction("RV2-Esegn", "Print possible accidental digestion paths",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PrintPossibleAccidentalDigestionPaths(Pawn predator)
        {
            if ((predator.PawnData()?.VoreTracker?.VoreTrackerRecords.Count ?? 0) == 0)
            {
                RV2Log.Message("Predator had no records to check");
                return;
            }
            
            var record = predator.PawnData().VoreTracker.VoreTrackerRecords[0];

            var paths = RV2_Common.VorePaths.FindAll(path =>
                path.voreGoal.IsLethal && path.stages.Any(stage => stage.jumpKey == record.CurrentVoreStage.def.jumpKey)
                                       && predator.PawnData().VoreTracker.VoreTrackerRecords.All(r =>
                                           r.VorePath.def.IsValid(predator, r.Prey, out _, true,
                                               RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionIgnoresDesignations)
                                       ));

            if (paths.Count == 0)
            {
                RV2Log.Message("No possible paths");
                return;
            }
            
            foreach (var path in paths)
            {
                RV2Log.Message(path.defName);
            }
        }
    }
}