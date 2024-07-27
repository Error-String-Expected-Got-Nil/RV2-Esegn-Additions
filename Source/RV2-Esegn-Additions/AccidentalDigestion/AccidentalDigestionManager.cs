using System.Collections.Generic;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionManager : GameComponent
    {
        public static AccidentalDigestionManager Manager;

        public Dictionary<Pawn, AccidentalDigestionTracker> Trackers =
            new Dictionary<Pawn, AccidentalDigestionTracker>();
        
        public AccidentalDigestionManager(Game game)
        {
            Manager = this;
        }
    }
}