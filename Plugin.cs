using HarmonyLib;
using KSP.OAB;
using Redux.ExtraModTypes;
using UnityEngine;

namespace VABSearchRedux;

public sealed class Plugin : KerbalMod
{
    public const string ModId = "com.javap.vabsearchredux";
    private Harmony? _harmony;

    public override void OnInitialized()
    {
        base.OnInitialized();
        _harmony = new Harmony(ModId);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Debug.Log("[VAB Search Redux] Craft-load search installed (1.1.1)");
    }

    private void OnDestroy() => _harmony?.UnpatchAll(ModId);
}

[HarmonyPatch(typeof(OABLoadDialog), "OnEnable")]
internal static class LoadDialogOpenedPatch
{
    private static void Postfix(OABLoadDialog __instance,
        Transform ____saveEntryParent, List<OABLoadDialogExistingSave> ____saveEntries)
    {
        var controller = __instance.GetComponent<CraftSearchController>();
        if (controller == null)
            controller = __instance.gameObject.AddComponent<CraftSearchController>();
        controller.Open(__instance, ____saveEntryParent, ____saveEntries);
    }
}

// Stock craft thumbnails finish loading asynchronously. Filter their rows too,
// rather than allowing late arrivals to bypass the current query.
[HarmonyPatch(typeof(OABLoadDialog), "CreateSaveEntry")]
internal static class CraftEntryCreatedPatch
{
    private static void Postfix(OABLoadDialog __instance)
        => __instance.GetComponent<CraftSearchController>()?.Refresh();
}

[HarmonyPatch(typeof(OABLoadDialog), "PopulateEntries")]
internal static class CraftEntriesRepopulatedPatch
{
    private static void Postfix(OABLoadDialog __instance)
        => __instance.GetComponent<CraftSearchController>()?.Refresh();
}

[HarmonyPatch(typeof(OABLoadDialog), "DeleteWorkspace")]
internal static class CraftEntryRemovedPatch
{
    private static void Postfix(OABLoadDialog __instance)
        => __instance.GetComponent<CraftSearchController>()?.Refresh();
}
