using System.Collections.Generic;
using RimVore2;
using RV2_Esegn_Additions.Utilities;
using Verse;

namespace RV2_Esegn_Additions;

public class RollAction_SkipIfCannotHeal : RollAction
{
    public override bool TryAction(VoreTrackerRecord curRecord, float rollStrength)
    {
        base.TryAction(curRecord, rollStrength);

        if (!TargetPawn.health.hediffSet.hediffs
                .Any(hediff => hediff.TendableNow() || (hediff is Hediff_Injury && !hediff.IsPermanent())))
            return false;
        
        if (!RV2_EADD_Settings.eadd.EnableEndoanalepticsSupplements) return true;
        var hediffeas = EndoanalepticsUtils.GetEndoanaleptics(curRecord.Predator);
        if (curRecord.Predator.PawnData()?.Designations.TryGetValue(RV2_EADD_Common.EaddDesignationDefOf.heal_wait)?
                .IsEnabled() == false && hediffeas == null)
            return false;

        return true;
    }
    
    public override IEnumerable<string> ConfigErrors()
    {
        foreach(var error in base.ConfigErrors())
        {
            yield return error;
        }
        if(target == VoreRole.Invalid)
        {
            yield return "required field \"target\" is not set";
        }
    }
}