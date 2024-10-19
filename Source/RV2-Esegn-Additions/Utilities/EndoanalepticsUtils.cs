using Verse;

namespace RV2_Esegn_Additions.Utilities;

public static class EndoanalepticsUtils
{
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
}