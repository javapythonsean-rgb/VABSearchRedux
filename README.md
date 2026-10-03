# VAB Craft Search Redux

Adds a search box above the craft list in the KSP2 Redux VAB **Load Workspace** dialog. This searches saved crafts, not parts.

Version 1.1.1 adds the dialog's blue-gray palette, cyan focus accents, its regular control font, 12px outer padding, 16px text padding, and subtle stepped pixel corners. A separate match-count badge keeps the search text aligned. Corner sprites are generated in memory with point filtering and fixed-size sliced corners; no extra assets or stock-file changes are needed.

- Type to filter by displayed craft name, workspace name, vehicle name, or filename.
- Matching is a case-insensitive substring; leading/trailing whitespace is ignored.
- Includes every craft already shown by the game, including stock crafts loaded asynchronously.
- Existing sorting, loading, merging, and deletion stay with the game's own controls.
- The count shows matching/total crafts, or "No matches".
- Ctrl+F focuses the search while the load dialog is open.
- Escape clears the query and releases focus. Clearing the query restores all crafts.
- Search resets when the dialog is reopened. A craft hidden by a query is deselected so it cannot accidentally be loaded, merged, or deleted.

No craft/save files or stock game files are modified. Version 1.1.0 replaces the mistaken parts-search implementation from 1.0.x, using the same mod ID and installation directory.

## Install

Requires KSP2 Redux with SpaceWarp2. With the game closed, extract the release ZIP into the game's `mods` folder. Replace the previous `mods/VABSearchRedux/` files if present. Do not install the old BepInEx VABSearch build alongside it.

## Build and verification

Requires a .NET 8 SDK and managed assemblies from an installed Redux build. Set `KSP2DIR` to that installation:

```powershell
dotnet build -c Release -p:KSP2DIR="C:\path\to\KSP2 Redux"
dotnet run --project tests/CraftQuery.Tests.csproj -c Release
```

This release is compile-checked against the local Redux assemblies and has offline query and pixel-corner tests. Its Harmony targets and entry-list behavior were inspected in the installed game's IL, and its palette/layout were inspected in the dialog prefab. The UI has **not** been visually tested in a running game.

Manual check: open VAB → Load Workspace, type a partial craft name, clear it, sort with a query active, try a query with no matches, and close/reopen the dialog. Confirm the search box fits the list and load/merge buttons only act on a visible selected craft. After Redux updates, rebuild if these APIs change.
