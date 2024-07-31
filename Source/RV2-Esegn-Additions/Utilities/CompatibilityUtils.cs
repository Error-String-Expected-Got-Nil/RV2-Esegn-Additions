// Purely exists to re-create the List<T>.Empty() extension method from RimWorld 1.5 in 1.4
#if v1_4
using System.Collections.Generic;

namespace RV2_Esegn_Additions.Utilities
{
    public static class CompatibilityUtils
    {
        public static bool Empty<T>(this List<T> list)
        {
            return list.Count == 0;
        }
    }
}
#endif