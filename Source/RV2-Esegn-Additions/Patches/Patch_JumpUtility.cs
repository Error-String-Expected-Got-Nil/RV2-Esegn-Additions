using System.Reflection;
using HarmonyLib;
using RimVore2;

namespace RV2_Esegn_Additions
{
    [HarmonyPatch]
    public class Patch_JumpUtility
    {
        public static bool SkipNextPathJumpNotification = false;

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
    }
}