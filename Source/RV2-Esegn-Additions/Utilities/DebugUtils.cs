#if v1_5
using LudeonTK;
#endif
using System.Linq;
using RimVore2;
using Verse;
using RimWorld;

namespace RV2_Esegn_Additions.Utilities;

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
        AccidentalDigestionManager.Manager.GetTracker(predator).BeginAccidentalDigestion();
    }

    [DebugAction("RV2-Esegn", "Resolve accidental digestion", 
        actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void ResolveAllAccidentalDigestion(Pawn predator)
    {
        // .ToList() is to duplicate the list in advance so there isn't an enumeration modification exception.
        AccidentalDigestionManager.Manager.GetTracker(predator, false)?.Records
            .ToList()
            .ForEach(record => record.ResolveAccidentalDigestion());
    }

    [DebugAction("RV2-Esegn", "Print AD tracker info", actionType = DebugActionType.ToolMapForPawns, 
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void PrintAccidentalDigestionTrackerInfo(Pawn predator)
    {
        RV2Log.Message("Printing accidental digestion tracker info for pawn " + predator.LabelShort + ":");
            
        var tracker = AccidentalDigestionManager.Manager.GetTracker(predator, false);

        if (tracker == null)
        {
            RV2Log.Message("N/A, pawn had no accidental digestion tracker");
            return;
        }
            
        RV2Log.Message("Cooldown: " + tracker.Cooldown);
        RV2Log.Message("Control mod: " + tracker.PredatorControlModifier);
        RV2Log.Message("Awareness mod: " + tracker.PredatorAwarenessModifier);
        RV2Log.Message("Num. records: " + tracker.Records.Count);
        RV2Log.Message("Can start AD: " + tracker.CanBeginAccidentalDigestion());
    }

    [DebugAction("RV2-Esegn", "Print AD record info", actionType = DebugActionType.ToolMapForPawns, 
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void PrintAccidentalDigestionRecordsInfo(Pawn predator)
    {
        RV2Log.Message("Printing accidental digestion record info for pawn " + predator.LabelShort + ":");
            
        var tracker = AccidentalDigestionManager.Manager.GetTracker(predator, false);

        if (tracker == null || tracker.Records.Empty())
        {
            RV2Log.Message("N/A, pawn had no accidental digestion records");
            return;
        }

        foreach (var record in tracker.Records)
        {
            RV2Log.Message("JumpKey: " + record.JumpKey);
            RV2Log.Message("VoreGoal: " + record.VoreGoal);
            RV2Log.Message("Prey: " + string.Join(", ", record.SwitchedRecords
                .Select(adr => adr.Prey.LabelShort)));
            RV2Log.Message("Can roll awareness: " + record.CanRollAwareness());
        }
    }

    [DebugAction("RV2-Esegn", "Test roll AD x100", actionType = DebugActionType.ToolMapForPawns, 
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void TestRollAccidentalDigestionStart(Pawn predator)
    {
        RV2Log.Message("Test-rolling beginning accidental digestion for pawn " + predator.LabelShort + 
                       " (will not actually start accidental digestion):");
            
        var tracker = AccidentalDigestionManager.Manager.GetTracker(predator, false);
            
        if (tracker == null)
        {
            RV2Log.Message("Could not test roll accidental digestion, target pawn had no accidental " +
                           "digestion tracker.");
            return;
        }

        bool succeeded = false;
        for (var i = 1; i <= 100; i++)
        {
            if (tracker.RollForAccidentalDigestion())
            {
                succeeded = true;
                RV2Log.Message("Roll " + i + " succeeded");
            }
        }
            
        if (!succeeded) RV2Log.Message("No rolls succeeded.");
    }

    [DebugAction("RV2-Esegn", "Test roll awareness x100", actionType = DebugActionType.ToolMapForPawns, 
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static void TestRollPredatorAwareness(Pawn predator)
    {
        RV2Log.Message("Test-rolling predator awareness for pawn " + predator.LabelShort + "'s first " +
                       "accidental digestion record (will not actually resolve accidental digestion):");
            
        var tracker = AccidentalDigestionManager.Manager.GetTracker(predator, false);

        if (tracker == null || tracker.Records.Empty())
        {
            RV2Log.Message("N/A, pawn had no accidental digestion records");
            return;
        }

        var record = tracker.Records[0];
            
        bool succeeded = false;
        for (var i = 1; i <= 100; i++)
        {
            if (record.RollAwareness())
            {
                succeeded = true;
                RV2Log.Message("Roll " + i + " succeeded");
            }
        }
            
        if (!succeeded) RV2Log.Message("No rolls succeeded.");
    }
}