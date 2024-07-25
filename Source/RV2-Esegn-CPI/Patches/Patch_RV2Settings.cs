using HarmonyLib;
using RimVore2;
using Verse;

namespace RV2_Esegn_CPI
{
    public class RV2_CPI_Mod : Mod
    {
        public static RV2_CPI_Mod mod;

        public RV2_CPI_Mod(ModContentPack content) : base(content)
        {
            mod = this;
            GetSettings<RV2_CPI_Settings>();
            WriteSettings();
        }
    }

    public class RV2_CPI_Settings : ModSettings
    {
        public static SettingsContainer_CPI cpi;

        public RV2_CPI_Settings()
        {
            cpi = new SettingsContainer_CPI();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref cpi, "RV2_CPI_SettingsContainer", new object[0]);
        }
    }
    
    [HarmonyPatch(typeof(RV2Settings), "DefsLoaded")]
    public static class Patch_RV2Settings_DefsLoaded
    {
        [HarmonyPostfix]
        private static void AddCPISettings()
        {
            RV2_CPI_Settings.cpi.DefsLoaded();
        }
    }

    [HarmonyPatch(typeof(RV2Settings), "Reset")]
    public static class Patch_RV2Settings_Reset
    {
        [HarmonyPostfix]
        private static void AddCPISettings()
        {
            RV2_CPI_Settings.cpi.Reset();
        }
    }

    [HarmonyPatch(typeof(RV2Mod), "WriteSettings")]
    public static class Patch_RV2Mod_WriteSettings
    {
        [HarmonyPostfix]
        private static void AddCPISettings()
        {
            RV2_CPI_Mod.mod.WriteSettings();
        }
    }

    [HarmonyPatch(typeof(Window_Settings), "InitializeTabs")]
    public static class Patch_Window_Settings
    {
        [HarmonyPostfix]
        private static void AddCPISettings()
        {
            Window_Settings.AddTab(new SettingsTab_CPI("RV2_CPI_SettingsTab".Translate(), 
                null, null));
        }
    }
}