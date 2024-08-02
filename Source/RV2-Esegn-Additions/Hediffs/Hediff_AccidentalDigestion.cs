using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RV2_Esegn_Additions
{
    public class Hediff_AccidentalDigestion : HediffWithComps
    {
        public AccidentalDigestionRecord LinkedRecord;
        private string _label;
        
        public override bool ShouldRemove => LinkedRecord == null;
        public override string Label => _label ?? UpdateLabel();

        public string UpdateLabel()
        {
            _label = string.Join("\n", LinkedRecord?.SwitchedRecords
                .Where(record => record.CurrentVoreStage.def.jumpKey == LinkedRecord.JumpKey)
                .Select(record => def.label + ": " + record.GetPreyName()) 
                                       ?? new List<string>());
            _label = _label == string.Empty ? def.label : _label;
            return _label;
        }
    }
}