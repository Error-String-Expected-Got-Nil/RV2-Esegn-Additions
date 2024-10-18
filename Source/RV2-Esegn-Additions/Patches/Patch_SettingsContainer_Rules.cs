using HarmonyLib;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2_Esegn_Additions;

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
        
    [HarmonyPatch(nameof(SettingsContainer_Rules.DesignationActive))]
    [HarmonyPrefix]
    public static bool Patch_DesignationActive(Pawn pawn, RV2DesignationDef designation, ref bool __result)
    {
        if (designation == RV2_EADD_Common.EaddDesignationDefOf.heal_wait)
        {
            if (pawn.IsAnimal() && pawn.Faction == Faction.OfPlayer)
                __result = !RV2_EADD_Settings.eadd.HealWaitDefaultPlayer;
            else if (pawn.IsColonistPlayerControlled || pawn.IsColonyMechPlayerControlled)
                __result = !RV2_EADD_Settings.eadd.HealWaitDefaultPlayer;
            else
                __result = !RV2_EADD_Settings.eadd.HealWaitDefaultOther;
                
            return false;
        }
            
        return true;
    }
}