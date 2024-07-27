using System.Collections.Generic;
using System.Linq;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionRecord : IExposable
    {
        public List<VoreTrackerRecord> OriginalRecords = new List<VoreTrackerRecord>();
        public List<VoreTrackerRecord> SwitchedRecords = new List<VoreTrackerRecord>();

        public Pawn Predator;
        public string JumpKey;
        
        public List<VorePathDef> PotentialTargetPaths()
        {
            Patch_VorePathDef.DisablePathConflictChecks = true;
            var ret = RV2_Common.VorePaths.FindAll(path =>
                path.voreGoal.IsLethal 
                && path.stages.Any(stage => stage.jumpKey == JumpKey)
                && OriginalRecords.All(record => record.VorePath.def.IsValid(Predator, record.Prey, out _, true, 
                    RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionIgnoresDesignations)
                )
            );
            Patch_VorePathDef.DisablePathConflictChecks = false;
            return ret;
        }
        
        public void ExposeData()
        {
            
        }
    }
}