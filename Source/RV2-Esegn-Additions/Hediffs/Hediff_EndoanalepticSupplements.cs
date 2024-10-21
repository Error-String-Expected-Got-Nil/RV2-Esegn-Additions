using System.Collections.Generic;
using System.Linq;
using RV2_Esegn_Additions.Utilities;
using UnityEngine;
using Verse;

#if v1_4
using static RV2_Esegn_Additions.Utilities.CompatibilityUtils;
#endif

namespace RV2_Esegn_Additions;

public class Hediff_EndoanalepticSupplements : HediffWithComps
{
    public List<ExposablePair> TendQualities = [];

    public override bool ShouldRemove => TendQualities.Empty();
    public override string Description => base.Description
                                          + "\n\nRemaining tends: " + TendQualities.Count
                                          + "\nAverage quality: " 
                                          + (TendQualities.Sum(quality => quality.First) 
                                             / TendQualities.Count)
                                          .ToStringPercent();

    // Returns a tuple of (base quality, max quality)
    public ExposablePair PopRandomTend()
    {
        var index = Random.Range(0, TendQualities.Count);
        var tendQuality = TendQualities[index];
        TendQualities.RemoveAt(index);

        if (TendQualities.Empty()) pawn.health.RemoveHediff(this);
        
        return tendQuality;
    }
    
    public override bool TryMergeWith(Hediff other)
    {
        if (other is not Hediff_EndoanalepticSupplements hediffeas || !base.TryMergeWith(other)) return false;
        
        TendQualities.AddRange(hediffeas.TendQualities);
        
        return true;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        
        Scribe_Collections.Look(ref TendQualities, nameof(TendQualities), LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.LoadingVars && TendQualities == null) TendQualities = [];
    }
}