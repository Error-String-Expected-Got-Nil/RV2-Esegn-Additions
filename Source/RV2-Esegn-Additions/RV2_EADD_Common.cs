using RimVore2;
using RimWorld;
using Verse;

namespace RV2_Esegn_Additions;

public class RV2_EADD_Common
{
    [DefOf]
    public static class EaddInteractionDefOf
    {
        static EaddInteractionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EaddInteractionDefOf));
        }

        public static InteractionDef RV2_EADD_AccidentalDigestionInteraction;
    }

    [DefOf]
    public static class EaddHediffDefOf
    {
        static EaddHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EaddHediffDefOf));
        }

        public static HediffDef RV2_EADD_AccidentalDigestionHediff;
        public static HediffDef RV2_EADD_EndoanalepticSupplementsHediff;
    }

    [DefOf]
    public static class EaddRecordDefOf
    {
        static EaddRecordDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EaddRecordDefOf));
        }

        public static RecordDef RV2_EADD_AccidentalDigestion_Predator;
        public static RecordDef RV2_EADD_AccidentalDigestion_Prey;
    }

    [DefOf]
    public static class EaddDesignationDefOf
    {
        static EaddDesignationDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EaddDesignationDefOf));
        }

        public static RV2DesignationDef heal_wait;
    }
}