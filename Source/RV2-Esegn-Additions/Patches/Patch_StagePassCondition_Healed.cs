using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions;

[HarmonyPatch(typeof(StagePassCondition_Healed))]
public class Patch_StagePassCondition_Healed
{
    private static readonly MethodInfo TargetPawnInfo = AccessTools.Method(typeof(TargetedStagePassCondition),
        "TargetPawn");

    private static readonly MethodInfo ModifyTotalSeverityInfo = AccessTools.Method(
        typeof(Patch_StagePassCondition_Healed), nameof(ModifyTotalSeverity));
    
    [HarmonyPatch(nameof(StagePassCondition_Healed.IsPassed))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Patch_IsPassed(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var cursor = new CodeMatcher(instructions, generator);

        // Transpiler procedure:
        // 1 - Seek to end
        // 2 - Search backwards for loading of float.MinValue constant
        // 3 - Seek back 2 instructions to first after totalInjurySeverity is first set
        // 4 - Insert instructions to call modification function
        
        return cursor
            .End() // 1
            .SearchBackwards(inst => inst.LoadsConstant(float.MinValue)) // 2
            .Advance(-2) // 3
            .Insert(
                CodeInstruction.LoadArgument(0), // StagePassCondition_Healed instance
                CodeInstruction.LoadArgument(1), // VoreTrackerRecord record
                new CodeInstruction(OpCodes.Call, TargetPawnInfo), // Target pawn of this condition
                CodeInstruction.LoadLocal(1, true), // totalInjurySeverity local (by ref)
                new CodeInstruction(OpCodes.Call, ModifyTotalSeverityInfo) // Call modification function
            ) // 4
            .Instructions();
    }

    private static void ModifyTotalSeverity(Pawn target, ref float totalSeverity)
    {
        if (!RV2_EADD_Settings.eadd.HealVoreWaitsForImmunity) return;

        totalSeverity += target.health.hediffSet.hediffs
            .Sum(hediff => 1.0f - (hediff.TryGetComp<HediffComp_Immunizable>()?.Immunity ?? 1.0f));
    }
}