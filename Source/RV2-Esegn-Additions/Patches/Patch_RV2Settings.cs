using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_Additions;

public class RV2_Esegn_Additions_Mod : Mod
{
    public static RV2_Esegn_Additions_Mod mod;

    public RV2_Esegn_Additions_Mod(ModContentPack content) : base(content)
    {
        mod = this;
        GetSettings<RV2_EADD_Settings>();
        WriteSettings();
    }
}

public class RV2_EADD_Settings : ModSettings
{
    public static SettingsContainer_EsegnAdditions eadd;

    public RV2_EADD_Settings()
    {
        eadd = new SettingsContainer_EsegnAdditions();
    }
        
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref eadd, "RV2_EADD_SettingsContainer");
    }
}
    
[HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
public static class Patch_RV2Settings_DefsLoaded
{
    [HarmonyPostfix]
    private static void AddEADDSettings()
    {
        RV2_EADD_Settings.eadd.DefsLoaded();
    }
}

[HarmonyPatch(typeof(RV2Settings), "Reset")]
public static class Patch_RV2Settings_Reset
{
    [HarmonyPostfix]
    private static void AddEADDSettings()
    {
        RV2_EADD_Settings.eadd.Reset();
    }
}

[HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
public static class Patch_RV2Mod_WriteSettings
{
    [HarmonyPostfix]
    private static void AddEADDSettings()
    {
        RV2_Esegn_Additions_Mod.mod.WriteSettings();
    }
}

[HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
public static class Patch_Window_Settings
{
    [HarmonyPostfix]
    private static void AddEADDSettings()
    {
        Window_Settings.AddTab(new SettingsTab_EsegnAdditions("RV2_EADD_SettingsTab".Translate(), 
            null, null));
    }
}