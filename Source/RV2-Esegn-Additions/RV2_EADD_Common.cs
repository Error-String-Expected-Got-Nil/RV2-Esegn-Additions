using RimWorld;
using Verse;

namespace RV2_Esegn_Additions
{
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
        }
    }
}