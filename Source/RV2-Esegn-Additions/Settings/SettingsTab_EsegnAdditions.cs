using System;
using RimVore2;
using UnityEngine;

namespace RV2_Esegn_Additions;

public class SettingsTab_EsegnAdditions : SettingsTab
{
    public SettingsTab_EsegnAdditions(string label, Action clickedAction, bool selected) 
        : base(label, clickedAction, selected) {}
        
    public SettingsTab_EsegnAdditions(string label, Action clickedAction, Func<bool> selected) 
        : base(label, clickedAction, selected) {}
        
    public override SettingsContainer AssociatedContainer => RV2_EADD_Settings.eadd;
    public SettingsContainer_EsegnAdditions EsegnAdditions 
        => (SettingsContainer_EsegnAdditions) AssociatedContainer;

    public override void FillRect(Rect inRect)
    {
        EsegnAdditions.FillRect(inRect);
    }
}