using RimWorld;

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
    }
}