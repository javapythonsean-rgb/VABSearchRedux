// Compile-only signatures from the public KSP2 Redux API documentation.
// This assembly is never included in the mod ZIP.
using TMPro;
using UnityEngine;

namespace Redux.ExtraModTypes
{
    public abstract class KerbalMod : MonoBehaviour
    {
        public virtual void OnInitialized() { }
    }
}

namespace KSP.OAB
{
    public class ObjectAssemblyUIEvents { }
    public class ObjectAssemblyBuilderEvents { }
    public class OABSessionInformation { }

    public class AssemblyPartsPicker : MonoBehaviour
    {
        public TMP_InputField searchText = null!;

        public virtual void Initialize(
            ObjectAssemblyUIEvents eventsToUse,
            ObjectAssemblyBuilderEvents builderEvents,
            OABSessionInformation statsToUse) { }

        public void ToggleSearch(bool toggled) { }
        public void UpdateSearch() { }
    }
}
