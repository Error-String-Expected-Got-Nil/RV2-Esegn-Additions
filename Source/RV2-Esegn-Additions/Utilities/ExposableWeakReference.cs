using System;
using Verse;

// Minor helper class for saving/loading weak references. I realized Scribe_References.Look() actually has an overload
// specifically for dealing with weak references, so the only practical modification here is there's a static predicate
// function for deciding if a weak reference should be saved, and this is easier to scribe in lists.
//
// Should be scribed with Scribe_Deep or LookMode.Deep!
namespace RV2_Esegn_Additions.Utilities
{
    public class ExposableWeakReference<T> : IExposable where T : class, ILoadReferenceable
    {
        private Verse.WeakReference<T> _weakRef;

        // Now null-coalescing because whatIf encountered a very strange bug involving _weakRef somehow becoming null
        public T Target => _weakRef?.Target;
        public bool IsAlive => _weakRef?.IsAlive ?? false;
        
        public ExposableWeakReference() {}
        
        public ExposableWeakReference(T target)
        {
            _weakRef = new Verse.WeakReference<T>(target);
        }
        
        public void ExposeData()
        {
            Scribe_References.Look(ref _weakRef, nameof(_weakRef));
        }
    }
}