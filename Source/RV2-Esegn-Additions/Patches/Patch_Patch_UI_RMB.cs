using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_Additions;

[HarmonyPatch]
public class Patch_Patch_UI_RMB
{
    private static readonly MethodInfo MoveNextInfo = AccessTools.Method(typeof(List<VoreTrackerRecord>.Enumerator),
        nameof(List<VoreTrackerRecord>.Enumerator.MoveNext));

    private static readonly FieldInfo JumpKeyInfo = AccessTools.Field(typeof(VoreStageDef),
        nameof(VoreStageDef.jumpKey));

    private static readonly MethodInfo RecordIsFromAccidentalDigestionInfo = AccessTools.Method(
        typeof(Patch_Patch_UI_RMB), nameof(RecordIsFromAccidentalDigestion));
        
    // The target class is internal and the target method is private, so we gotta do it like this
    [HarmonyTargetMethod]
    public static MethodBase Target()
    {
        return AccessTools.Method("RimVore2.RV2_Patch_UI_RMB_AddHumanlikeOrders:DoStageJumpOption");
    }

    [HarmonyPrefix]
    public static void Patch_DoStageJumpOption_Prefix()
    {
        Patch_VorePathDef.DisablePathConflictChecks = true;
    }
        
    [HarmonyPostfix]
    public static void Patch_DoStageJumpOption_Postfix()
    {
        Patch_VorePathDef.DisablePathConflictChecks = false;
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Patch_DoStageJumpOption_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var cursor = new CodeMatcher(instructions, generator);
            
        // Transpiler procedure:
        // 1 - Search for call to ".MoveNext()"
        // 2 - Place label on previous instruction; jumping to will be equivalent to "continue" statement 
        // 3 - Search backwards for loading of ".jumpKey" field (at top of loop)
        // 4 - Things get a little weird.
        //     So, it seems like there's a sort of "phantom class" made to hold the "record" variable for the loop.
        //     I assume some CIL fuckery is the reason for that. Regardless, loading the "record" variable onto the
        //     stack is now non-trivial... but the existing IL *does* already do it for us. So what we can do
        //     instead is duplicate it, and then use the duplicate. Once done with that, we can pop the extra
        //     if it turns out we want to continue the loop.
        //     That said: Step 4 is to seek to the first instruction after the record is loaded onto the stack.
        // 5 - Make label to continue as-normal for the function if check was false
        // 6 - Full code for checking if record is from with accidental digestion.
        //     Duplicate the record; Call the check; If false, continue as usual; Otherwise, remove the extra
        //     record object from the stack, and branch to the continue label

        return cursor
            .SearchForward(inst => inst.Calls(MoveNextInfo)) // 1
            .Advance(-1)
            .CreateLabel(out var continueLabel) // 2
            .SearchBackwards(inst => inst.LoadsField(JumpKeyInfo)) // 3
            .Advance(-2) // 4
            .CreateLabel(out var checkFalse) // 5
            .Insert(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, RecordIsFromAccidentalDigestionInfo),
                new CodeInstruction(OpCodes.Brfalse, checkFalse),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Br, continueLabel) // 6
            )
            .Instructions();
    }

    // Returns true if record is the result of accidental digestion
    private static bool RecordIsFromAccidentalDigestion(VoreTrackerRecord record)
    {
        return AccidentalDigestionManager.Manager.GetTracker(record.Predator, false)?.Records
            .Any(adrecord => adrecord.SwitchedRecords.Contains(record)) ?? false;
    }
}