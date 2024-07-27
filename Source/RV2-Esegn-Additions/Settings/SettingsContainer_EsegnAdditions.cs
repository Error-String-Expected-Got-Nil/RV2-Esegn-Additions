using System;
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
        private BoolSmartSetting pathConflictsIgnoreDesignations;

        private BoolSmartSetting enableAccidentalDigestion;
        private FloatSmartSetting baseAccidentalDigestionTickChance;
        private BoolSmartSetting accidentalDigestionIgnoresDesignations;

        public bool EnableVorePathConflicts => enableVorePathConflicts.value;
        public bool AllowConflictingManualInteractions => allowConflictingManualInteractions.value;
        public bool AllowGoalSwitchersToProposeConflicting => allowGoalSwitchersToProposeConflicting.value;
        public bool PathConflictsIgnoreDesignations => pathConflictsIgnoreDesignations.value;

        public bool EnableAccidentalDigestion => enableAccidentalDigestion.value;
        public float BaseAccidentalDigestionTickChance => baseAccidentalDigestionTickChance.value / 100f;
        public bool AccidentalDigestionIgnoresDesignations => accidentalDigestionIgnoresDesignations.value;

        public override void Reset()
        {
            enableVorePathConflicts = null;
            allowConflictingManualInteractions = null;
            allowGoalSwitchersToProposeConflicting = null;
            pathConflictsIgnoreDesignations = null;

            enableAccidentalDigestion = null;
            baseAccidentalDigestionTickChance = null;
            accidentalDigestionIgnoresDesignations = null;
            
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
            if (pathConflictsIgnoreDesignations == null || pathConflictsIgnoreDesignations.IsInvalid())
                pathConflictsIgnoreDesignations = new BoolSmartSetting(
                    "RV2_EADD_Settings_PathConflictsIgnoreDesignations", false, false,
                    "RV2_EADD_Settings_PathConflictsIgnoreDesignations_Tip");

            if (enableAccidentalDigestion == null || enableAccidentalDigestion.IsInvalid())
                enableAccidentalDigestion = new BoolSmartSetting(
                    "RV2_EADD_Settings_EnableAccidentalDigestion", true, true, 
                    "RV2_EADD_Settings_EnableAccidentalDigestion_Tip");
            if (baseAccidentalDigestionTickChance == null || baseAccidentalDigestionTickChance.IsInvalid())
                baseAccidentalDigestionTickChance = new FloatSmartSetting(
                    "RV2_EADD_Settings_BaseAccidentalDigestionTickChance", 0.46f, 0.46f,
                    0f, 100f, "RV2_EADD_Settings_BaseAccidentalDigestionTickChance_Tip", 
                    "0.00", "%");
            if (accidentalDigestionIgnoresDesignations == null || accidentalDigestionIgnoresDesignations.IsInvalid())
                accidentalDigestionIgnoresDesignations = new BoolSmartSetting(
                    "RV2_EADD_Settings_AccidentalDigestionIgnoresDesignations", false, false,
                    "RV2_EADD_Settings_AccidentalDigestionIgnoresDesignations_Tip");
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

            list.HeaderLabel("RV2_EADD_Settings_VorePathConflictsHeader".Translate());
            list.Gap();
            
            enableVorePathConflicts.DoSetting(list);
            allowConflictingManualInteractions.DoSetting(list);
            allowGoalSwitchersToProposeConflicting.DoSetting(list);
            pathConflictsIgnoreDesignations.DoSetting(list);
            
            list.Gap();
            list.HeaderLabel("RV2_EADD_Settings_AccidentalDigestionHeader".Translate());
            list.Gap();
            
            enableAccidentalDigestion.DoSetting(list);
            baseAccidentalDigestionTickChance.DoSetting(list);
            list.Label(
                "RV2_EADD_Settings_AccidentalDigestionChanceExample".Translate(18, 
                    Math.Round((1f - Math.Pow(1f - BaseAccidentalDigestionTickChance, 18)) * 100f, 
                        2)),
                -1,
                "RV2_EADD_Settings_AccidentalDigestionChanceExample_Tip".Translate());
            list.Label(
                "RV2_EADD_Settings_AccidentalDigestionChanceExample".Translate(36,
                    Math.Round((1f - Math.Pow(1f - BaseAccidentalDigestionTickChance, 36)) * 100f, 
                        2)),
                -1,
                "RV2_EADD_Settings_AccidentalDigestionChanceExample_Tip".Translate());
            accidentalDigestionIgnoresDesignations.DoSetting(list);

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
            Scribe_Deep.Look(ref allowGoalSwitchersToProposeConflicting, "AllowGoalSwitchersToProposeConflicting", 
                new object[0]);
            Scribe_Deep.Look(ref pathConflictsIgnoreDesignations, "PathConflictsIgnoreDesignations", 
                new object[0]);

            Scribe_Deep.Look(ref enableAccidentalDigestion, "EnableAccidentalDigestion", new object[0]);
            Scribe_Deep.Look(ref baseAccidentalDigestionTickChance, "BaseAccidentalDigestionTickChance", 
                new object[0]);
            Scribe_Deep.Look(ref accidentalDigestionIgnoresDesignations, 
                "AccidentalDigestionIgnoresDesignations", new object[0]);
            
            PostExposeData();
        }
    }
}