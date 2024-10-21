using System;
using RimVore2;
using UnityEngine;
using Verse;

namespace RV2_Esegn_Additions;

public class SettingsContainer_EsegnAdditions : SettingsContainer
{
    private BoolSmartSetting enableVorePathConflicts;
    private BoolSmartSetting allowConflictingManualInteractions;
    private BoolSmartSetting allowGoalSwitchersToProposeConflicting;
    private BoolSmartSetting pathConflictsIgnoreDesignations;

    private BoolSmartSetting enableAccidentalDigestion;
    private FloatSmartSetting baseAccidentalDigestionTickChance;
    private BoolSmartSetting accidentalDigestionIgnoresDesignations;
    private EnumSmartSetting<NotificationType> accidentalDigestionNotificationType;
    private FloatSmartSetting basePredatorAwarenessChance;
    private BoolSmartSetting preyMustStruggleToBeNoticed;
    private BoolSmartSetting canAlwaysAccidentallyDigest;
    private BoolSmartSetting longTermPreventsAccidentalDigestion;
    private FloatSmartSetting accidentalDigestionCooldown;
    private BoolSmartSetting allowAwarenessRolls;
    private FloatSmartSetting abortDigestionThreshold;

    private BoolSmartSetting enableEndoanalepticsSupplements;
    private BoolSmartSetting healWaitDefaultPlayer;
    private BoolSmartSetting healWaitDefaultOther;
    private BoolSmartSetting endoanalepticsSkipWarmup;
    private BoolSmartSetting healVoreWaitsForImmunity;

    public bool EnableVorePathConflicts => enableVorePathConflicts.value;
    public bool AllowConflictingManualInteractions => allowConflictingManualInteractions.value;
    public bool AllowGoalSwitchersToProposeConflicting => allowGoalSwitchersToProposeConflicting.value;
    public bool PathConflictsIgnoreDesignations => pathConflictsIgnoreDesignations.value;
        
    public bool EnableAccidentalDigestion => enableAccidentalDigestion.value;
    public float BaseAccidentalDigestionTickChance => baseAccidentalDigestionTickChance.value / 100f;
    public bool AccidentalDigestionIgnoresDesignations => accidentalDigestionIgnoresDesignations.value;
    public NotificationType AccidentalDigestionNotificationType => accidentalDigestionNotificationType.value;
    public float BasePredatorAwarenessChance => basePredatorAwarenessChance.value / 100f;
    public bool PreyMustStruggleToBeNoticed => preyMustStruggleToBeNoticed.value;
    public bool CanAlwaysAccidentallyDigest => canAlwaysAccidentallyDigest.value;
    public bool LongTermPreventsAccidentalDigestion => longTermPreventsAccidentalDigestion.value;
    public uint AccidentalDigestionCooldown => (uint) accidentalDigestionCooldown.value;
    public bool AllowAwarenessRolls => allowAwarenessRolls.value;
    public float AbortDigestionThreshold => abortDigestionThreshold.value / 100f;

    public bool EnableEndoanalepticsSupplements => enableEndoanalepticsSupplements.value;
    public bool HealWaitDefaultPlayer => healWaitDefaultPlayer.value;
    public bool HealWaitDefaultOther => healWaitDefaultOther.value;
    public bool EndoanalepticsSkipWarmup => endoanalepticsSkipWarmup.value;
    public bool HealVoreWaitsForImmunity => healVoreWaitsForImmunity.value;

    public override void Reset()
    {
        enableVorePathConflicts = null;
        allowConflictingManualInteractions = null;
        allowGoalSwitchersToProposeConflicting = null;
        pathConflictsIgnoreDesignations = null;

        enableAccidentalDigestion = null;
        baseAccidentalDigestionTickChance = null;
        accidentalDigestionIgnoresDesignations = null;
        accidentalDigestionNotificationType = null;
        basePredatorAwarenessChance = null;
        preyMustStruggleToBeNoticed = null;
        canAlwaysAccidentallyDigest = null;
        longTermPreventsAccidentalDigestion = null;
        accidentalDigestionCooldown = null;
        allowAwarenessRolls = null;
        abortDigestionThreshold = null;

        enableEndoanalepticsSupplements = null;
        healWaitDefaultPlayer = null;
        healWaitDefaultOther = null;
        endoanalepticsSkipWarmup = null;
        healVoreWaitsForImmunity = null;
            
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
                "RV2_EADD_Settings_BaseAccidentalDigestionTickChance", 
                1.50f, 1.50f, 0f, 100f, 
                "RV2_EADD_Settings_BaseAccidentalDigestionTickChance_Tip", 
                "0.00", "%");
        if (accidentalDigestionIgnoresDesignations == null || accidentalDigestionIgnoresDesignations.IsInvalid())
            accidentalDigestionIgnoresDesignations = new BoolSmartSetting(
                "RV2_EADD_Settings_AccidentalDigestionIgnoresDesignations", false, false,
                "RV2_EADD_Settings_AccidentalDigestionIgnoresDesignations_Tip");
        if (accidentalDigestionNotificationType == null || accidentalDigestionNotificationType.IsInvalid())
            accidentalDigestionNotificationType = new EnumSmartSetting<NotificationType>(
                "RV2_EADD_Settings_AccidentalDigestionNotificationType",
                NotificationType.MessageNeutral, NotificationType.MessageNeutral,
                "RV2_EADD_Settings_AccidentalDigestionNotificationType_Tip");
        if (basePredatorAwarenessChance == null || basePredatorAwarenessChance.IsInvalid())
            basePredatorAwarenessChance = new FloatSmartSetting(
                "RV2_EADD_Settings_BasePredatorAwarenessChance",
                2.85f, 2.85f, 0f, 100f,
                "RV2_EADD_Settings_BasePredatorAwarenessChance_Tip", "0.00", "%");
        if (preyMustStruggleToBeNoticed == null || preyMustStruggleToBeNoticed.IsInvalid())
            preyMustStruggleToBeNoticed = new BoolSmartSetting(
                "RV2_EADD_Settings_PreyMustStruggleToBeNoticed", true, true,
                "RV2_EADD_Settings_PreyMustStruggleToBeNoticed_Tip");
        if (canAlwaysAccidentallyDigest == null || canAlwaysAccidentallyDigest.IsInvalid())
            canAlwaysAccidentallyDigest = new BoolSmartSetting(
                "RV2_EADD_Settings_CanAlwaysAccidentallyDigest", false, false,
                "RV2_EADD_Settings_CanAlwaysAccidentallyDigest_Tip");
        if (longTermPreventsAccidentalDigestion == null || longTermPreventsAccidentalDigestion.IsInvalid())
            longTermPreventsAccidentalDigestion = new BoolSmartSetting(
                "RV2_EADD_Settings_LongTermPreventsAccidentalDigestion", true, true,
                "RV2_EADD_Settings_LongTermPreventsAccidentalDigestion_Tip");
        if (accidentalDigestionCooldown == null || accidentalDigestionCooldown.IsInvalid())
            accidentalDigestionCooldown = new FloatSmartSetting(
                "RV2_EADD_Settings_AccidentalDigestionCooldown",
                30000, 30000, 0, 120000,
                "RV2_EADD_Settings_AccidentalDigestionCooldown_Tip", "0");
        if (allowAwarenessRolls == null || allowAwarenessRolls.IsInvalid())
            allowAwarenessRolls = new BoolSmartSetting(
                "RV2_EADD_Settings_AllowAwarenessRolls", true, true,
                "RV2_EADD_Settings_AllowAwarenessRolls_Tip");
        if (abortDigestionThreshold == null || abortDigestionThreshold.IsInvalid())
            abortDigestionThreshold = new FloatSmartSetting(
                "RV2_EADD_Settings_AbortDigestionThreshold",
                0, 0, 0, 100,
                "RV2_EADD_Settings_AbortDigestionThreshold_Tip", "0.00", "%");

        if (enableEndoanalepticsSupplements == null || enableEndoanalepticsSupplements.IsInvalid())
            enableEndoanalepticsSupplements = new BoolSmartSetting(
                "RV2_EADD_Settings_EnableEndoanalepticsSupplements", true, true,
                "RV2_EADD_Settings_EnableEndoanalepticsSupplements_Tip");
        if (healWaitDefaultPlayer == null || healWaitDefaultPlayer.IsInvalid())
            healWaitDefaultPlayer = new BoolSmartSetting(
                "RV2_EADD_Settings_HealWaitDefaultPlayer", true, true,
                "RV2_EADD_Settings_HealWaitDefaultPlayer_Tip");
        if (healWaitDefaultOther == null || healWaitDefaultOther.IsInvalid())
            healWaitDefaultOther = new BoolSmartSetting(
                "RV2_EADD_Settings_HealWaitDefaultOther", true, true,
                "RV2_EADD_Settings_HealWaitDefaultOther_Tip");
        if (endoanalepticsSkipWarmup == null || endoanalepticsSkipWarmup.IsInvalid())
            endoanalepticsSkipWarmup = new BoolSmartSetting(
                "RV2_EADD_Settings_EndoanalepticsSkipWarmup", true, true,
                "RV2_EADD_Settings_EndoanalepticsSkipWarmup_Tip");
        if (healVoreWaitsForImmunity == null || healVoreWaitsForImmunity.IsInvalid())
            healVoreWaitsForImmunity = new BoolSmartSetting(
                "RV2_EADD_Settings_HealVoreWaitsForImmunity", true, true,
                "RV2_EADD_Settings_HealVoreWaitsForImmunity_Tip");
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
                ChanceInRolls(18, BaseAccidentalDigestionTickChance)), -1,
            "RV2_EADD_Settings_AccidentalDigestionChanceExample_Tip".Translate());
        list.Label(
            "RV2_EADD_Settings_AccidentalDigestionChanceExample".Translate(36,
                ChanceInRolls(36, BaseAccidentalDigestionTickChance)), -1,
            "RV2_EADD_Settings_AccidentalDigestionChanceExample_Tip".Translate());
        accidentalDigestionIgnoresDesignations.DoSetting(list);
        accidentalDigestionNotificationType.DoSetting(list);
        canAlwaysAccidentallyDigest.DoSetting(list);
        longTermPreventsAccidentalDigestion.DoSetting(list);
        accidentalDigestionCooldown.DoSetting(list);
        preyMustStruggleToBeNoticed.DoSetting(list);
        allowAwarenessRolls.DoSetting(list);
        basePredatorAwarenessChance.DoSetting(list);
        list.Label(
            "RV2_EADD_Settings_PredatorAwarenessChanceExample".Translate(
                ChanceInRolls(24, BasePredatorAwarenessChance)), -1,
            "RV2_EADD_Settings_PredatorAwarenessChanceExample_Tip".Translate());
        abortDigestionThreshold.DoSetting(list);
            
        list.Gap();
        list.HeaderLabel("RV2_EADD_Settings_ExtrasHeader".Translate());
        list.Gap();
            
        enableEndoanalepticsSupplements.DoSetting(list);
        healWaitDefaultPlayer.DoSetting(list);
        healWaitDefaultOther.DoSetting(list);
        endoanalepticsSkipWarmup.DoSetting(list);
        healVoreWaitsForImmunity.DoSetting(list);

        list.Gap();

        list.EndScrollView(ref height, ref heightStale);
    }

    // Calculates the total chance in numRolls rolls that at least one of the rolls will succeed, when each has the
    // given 'chance' of happening. Also formats it as a percentage with two decimal places.
    private static double ChanceInRolls(uint numRolls, float chance)
    {
        return Math.Round((1f - Math.Pow(1f - chance, numRolls)) * 100f, 2);
    }
        
    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
        {
            EnsureSmartSettingDefinition();
        }

        Scribe_Deep.Look(ref enableVorePathConflicts, nameof(enableVorePathConflicts));
        Scribe_Deep.Look(ref allowConflictingManualInteractions, nameof(allowConflictingManualInteractions));
        Scribe_Deep.Look(ref allowGoalSwitchersToProposeConflicting, nameof(allowGoalSwitchersToProposeConflicting));
        Scribe_Deep.Look(ref pathConflictsIgnoreDesignations, nameof(pathConflictsIgnoreDesignations));

        Scribe_Deep.Look(ref enableAccidentalDigestion, nameof(enableAccidentalDigestion));
        Scribe_Deep.Look(ref baseAccidentalDigestionTickChance, nameof(baseAccidentalDigestionTickChance));
        Scribe_Deep.Look(ref accidentalDigestionIgnoresDesignations, nameof(accidentalDigestionIgnoresDesignations));
        Scribe_Deep.Look(ref accidentalDigestionNotificationType, nameof(accidentalDigestionNotificationType));
        Scribe_Deep.Look(ref basePredatorAwarenessChance, nameof(basePredatorAwarenessChance));
        Scribe_Deep.Look(ref preyMustStruggleToBeNoticed, nameof(preyMustStruggleToBeNoticed));
        Scribe_Deep.Look(ref canAlwaysAccidentallyDigest, nameof(canAlwaysAccidentallyDigest));
        Scribe_Deep.Look(ref longTermPreventsAccidentalDigestion, nameof(longTermPreventsAccidentalDigestion));
        Scribe_Deep.Look(ref accidentalDigestionCooldown, nameof(accidentalDigestionCooldown));
        Scribe_Deep.Look(ref allowAwarenessRolls, nameof(allowAwarenessRolls));
        Scribe_Deep.Look(ref abortDigestionThreshold, nameof(abortDigestionThreshold));
        
        Scribe_Deep.Look(ref enableEndoanalepticsSupplements, nameof(enableEndoanalepticsSupplements));
        Scribe_Deep.Look(ref healWaitDefaultPlayer, nameof(healWaitDefaultPlayer));
        Scribe_Deep.Look(ref healWaitDefaultOther, nameof(healWaitDefaultOther));
        Scribe_Deep.Look(ref endoanalepticsSkipWarmup, nameof(endoanalepticsSkipWarmup));
        Scribe_Deep.Look(ref healVoreWaitsForImmunity, nameof(healVoreWaitsForImmunity));
            
        PostExposeData();
    }
}