using System;
using RimVore2;
using UnityEngine;

namespace RV2_Esegn_CPI
{
    public class SettingsTab_CPI : SettingsTab
    {
        public SettingsTab_CPI(string label, Action clickedAction, bool selected) 
            : base(label, clickedAction, selected) {}
        
        public SettingsTab_CPI(string label, Action clickedAction, Func<bool> selected) 
            : base(label, clickedAction, selected) {}
        
        public override SettingsContainer AssociatedContainer => RV2_CPI_Settings.cpi;
        public SettingsContainer_CPI CPI => (SettingsContainer_CPI) AssociatedContainer;

        public override void FillRect(Rect inRect)
        {
            CPI.FillRect(inRect);
        }
    }
}