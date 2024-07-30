using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_Additions
{
    
    [HarmonyPatch(typeof(SettingsContainer_Rules))]
    public class Patch_SettingsContainer_Rules
    {
        public static bool MakeNextShouldStruggleCheckForced = false;

        [HarmonyPatch(nameof(SettingsContainer_Rules.ShouldStruggle))]
        [HarmonyPrefix]
        public static void Patch_ShouldStruggle(ref bool isForced)
        {
            if (!MakeNextShouldStruggleCheckForced) return;
            isForced = true;
            MakeNextShouldStruggleCheckForced = false;
        }
    }
}