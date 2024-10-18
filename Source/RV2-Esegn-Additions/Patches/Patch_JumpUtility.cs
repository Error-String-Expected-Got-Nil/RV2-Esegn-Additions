using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions;

[HarmonyPatch]
public class Patch_JumpUtility
{
    public static bool SkipNextPathJumpNotification = false;
    public static bool VoreJumpFlag = false;

    // DoNotification is private so we can't use nameof() here
    [HarmonyPatch(typeof(VoreJump), "DoNotification")]
    [HarmonyPrefix]
    public static bool Patch_DoNotification()
    {
        if (SkipNextPathJumpNotification)
        {
            SkipNextPathJumpNotification = false;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(VoreJump), nameof(VoreJump.Jump))]
    [HarmonyPrefix]
    public static void Patch_Jump_Prefix()
    {
        VoreJumpFlag = true;
    }
        
    [HarmonyPatch(typeof(VoreJump), nameof(VoreJump.Jump))]
    [HarmonyPostfix]
    public static void Patch_Jump_Postfix()
    {
        VoreJumpFlag = false;
    }
}