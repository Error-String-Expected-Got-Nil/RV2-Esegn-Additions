using RimVore2;
using Verse;

namespace RV2_Esegn_Additions.Utilities;

public static class EndoanalepticsUtils
{
    private static readonly VoreTargetSelectorRequest HealVoreSelector = new()
    {
        voreGoal = VoreGoalDefOf.Heal, 
        allMustMatch = true
    };
    
    public static void AddTend(Pawn pawn, float tendQuality)
    {
        var hediff = (Hediff_EndoanalepticSupplements)HediffMaker.MakeHediff(
            RV2_EADD_Common.EaddHediffDefOf.RV2_EADD_EndoanalepticSupplementsHediff, pawn);
        hediff.TendQualities.Add(tendQuality);
        pawn.health.AddHediff(hediff);
    }

    public static Hediff_EndoanalepticSupplements GetEndoanaleptics(Pawn pawn)
    {
        return (Hediff_EndoanalepticSupplements)pawn.health.hediffSet.hediffs.Find(hediff => 
            hediff.def == RV2_EADD_Common.EaddHediffDefOf.RV2_EADD_EndoanalepticSupplementsHediff);
    }

    public static bool CanDoHealVore(Pawn predator)
    {
        if (!RV2Mod.Settings.features.EndoVoreEnabled) return false;
        if (!predator.CanBePredator(out _)) return false;

        if (!RV2Mod.Settings.features.VoreQuirksEnabled) return true;
        
        var quirks = predator.QuirkManager(false);
        if (quirks == null) return false;
        if (quirks.HasSpecialFlag("FatalPredatorOnly")) return false;
        if (!quirks.HasVoreEnabler(HealVoreSelector)) return false;

        return true;
    }
}