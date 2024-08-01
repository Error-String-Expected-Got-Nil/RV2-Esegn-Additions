using System.Collections.Generic;
using RimVore2;
using Verse;

#if v1_4
using static RV2_Esegn_Additions.Utilities.CompatibilityUtils;
#endif

namespace RV2_Esegn_Additions
{
    public class AccidentalDigestionTracker : IExposable
    {
        public List<AccidentalDigestionRecord> Records = new List<AccidentalDigestionRecord>();

        public bool IsEmpty => Records.Empty();
        public uint Cooldown = 0;

        public void BeginCooldown()
        {
            Cooldown = RV2_EsegnAdditions_Settings.eadd.AccidentalDigestionCooldown;
        }

        public void TickCooldown()
        {
            if (Cooldown > 0) Cooldown--;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Records, nameof(Records), LookMode.Deep);
            Scribe_Values.Look(ref Cooldown, nameof(Cooldown));
        }
    }
}