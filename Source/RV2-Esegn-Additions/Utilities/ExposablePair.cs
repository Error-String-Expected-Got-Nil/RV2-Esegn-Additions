using Verse;

namespace RV2_Esegn_Additions.Utilities;

// Because there isn't a Scribe_Values for value tuples and I don't want to make one right now.
public struct ExposablePair : IExposable
{
    public float First;
    public float Second;
    
    public ExposablePair() { }

    public ExposablePair(float first, float second)
    {
        First = first;
        Second = second;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref First, nameof(First));
        Scribe_Values.Look(ref Second, nameof(Second));
    }
}