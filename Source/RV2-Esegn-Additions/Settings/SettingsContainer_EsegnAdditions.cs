using RimVore2;
using UnityEngine;
using Verse;

namespace RV2_Esegn_Additions
{
    public class SettingsContainer_EsegnAdditions : SettingsContainer
    {
        private BoolSmartSetting enableVorePathConflicts;
        private BoolSmartSetting allowConflictingManualInteractions;
        private BoolSmartSetting allowGoalSwitchersToProposeConflicting;

        public bool EnableVorePathConflicts => enableVorePathConflicts.value;
        public bool AllowConflictingManualInteractions => allowConflictingManualInteractions.value;
        public bool AllowGoalSwitchersToProposeConflicting => allowGoalSwitchersToProposeConflicting.value;

        public override void Reset()
        {
            enableVorePathConflicts = null;
            allowConflictingManualInteractions = null;
            allowGoalSwitchersToProposeConflicting = null;
            
            EnsureSmartSettingDefinition();
        }

        public override void EnsureSmartSettingDefinition()
        {
            if (enableVorePathConflicts == null || enableVorePathConflicts.IsInvalid())
                enableVorePathConflicts = new BoolSmartSetting("RV2_EADD_Settings_EnableVorePathConflicts",
                    true, true, "RV2_EADD_Settings_EnableVorePathConflicts_Tip");
            if (allowConflictingManualInteractions == null || allowConflictingManualInteractions.IsInvalid())
                allowConflictingManualInteractions = new BoolSmartSetting(
                    "RV2_EADD_Settings_AllowConflictingManualInteractions", false, false,
                    "RV2_EADD_Settings_AllowConflictingManualInteractions_Tip");
            if (allowGoalSwitchersToProposeConflicting == null || allowGoalSwitchersToProposeConflicting.IsInvalid())
                allowGoalSwitchersToProposeConflicting = new BoolSmartSetting(
                    "RV2_EADD_Settings_AllowGoalSwitchersToProposeConflicting", true, true,
                    "RV2_EADD_Settings_AllowGoalSwitchersToProposeConflicting_Tip");

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
            allowConflictingManualInteractions.DoSetting(list);
            allowGoalSwitchersToProposeConflicting.DoSetting(list);

            list.EndScrollView(ref height, ref heightStale);
        }
        
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                EnsureSmartSettingDefinition();
            }

            Scribe_Deep.Look(ref enableVorePathConflicts, "EnableVorePathConflicts", new object[0]);
            Scribe_Deep.Look(ref allowConflictingManualInteractions, "AllowConflictingManualInteractions",
                new object[0]);
            Scribe_Deep.Look(ref allowGoalSwitchersToProposeConflicting,
                "AllowGoalSwitchersToProposeConflicting", new object[0]);
            
            PostExposeData();
        }
    }
}