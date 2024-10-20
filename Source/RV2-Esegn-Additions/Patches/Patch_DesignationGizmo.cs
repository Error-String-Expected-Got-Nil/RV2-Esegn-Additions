using System.Reflection;
using HarmonyLib;
using RimVore2;
using RV2_Esegn_Additions.Utilities;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(DesignationGizmo))]
public class Patch_DesignationGizmo
{
    private static readonly FieldInfo ActiveDesignationInfo = AccessTools.Field(typeof(DesignationGizmo),
        "activeDesignation");
        
    [HarmonyPatch(nameof(DesignationGizmo.IsVisible))]
    [HarmonyPostfix]
    public static void Patch_IsVisible(DesignationGizmo __instance, ref bool __result)
    {
        if (__result == false) return;
            
        var designation = (RV2Designation)ActiveDesignationInfo.GetValue(__instance);

        if (designation.def != RV2_EADD_Common.EaddDesignationDefOf.heal_wait) return;

        if (!RV2_EADD_Settings.eadd.EnableEndoanalepticsSupplements)
        {
            __result = false;
            return;
        }

        __result = EndoanalepticsUtils.CanDoHealVore(designation.pawn);
    }
}