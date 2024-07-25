using RimVore2;
using UnityEngine;
using Verse;

namespace RV2_Esegn_CPI
{
    public class SettingsContainer_CPI : SettingsContainer
    {
        private BoolSmartSetting enableVorePathConflicts;

        public bool EnableVorePathConflicts => enableVorePathConflicts.value;

        public override void Reset()
        {
            enableVorePathConflicts = null;
            
            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if (enableVorePathConflicts == null || enableVorePathConflicts.IsInvalid())
            {
                enableVorePathConflicts = new BoolSmartSetting("RV2_CPI_Settings_EnableVorePathConflicts",
                    true, true, "RV2_CPI_Settings_EnableVorePathConflicts_Tip");
            }
        }
        
        private bool heightStale = true;
        private float height = 0f;
        private Vector2 scrollPosition;
        public void FillRect(Rect inRect)
        {
            Rect outerRect = inRect;
            UIUtility.MakeAndBeginScrollView(outerRect, height, ref scrollPosition, out Listing_Standard list);

            if (list.ButtonText("RV2_Settings_Reset".Translate()))
                Reset();

            enableVorePathConflicts.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }
        
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }

            Scribe_Deep.Look(ref enableVorePathConflicts, "EnableVorePathConflicts", new object[0]);

            PostExposeData();
        }
    }
}