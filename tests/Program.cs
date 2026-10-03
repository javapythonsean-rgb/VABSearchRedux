using VABSearchRedux;

int passed = 0;
void Check(string name, bool expected, string? query, string? label = null,
    string? workspace = null, string? vehicle = null, string? file = null)
{
    bool actual = CraftQuery.Matches(query, label, workspace, vehicle, file);
    if (actual != expected) throw new Exception($"{name}: expected {expected}, got {actual}");
    passed++;
}

Check("null query", true, null);
Check("empty query", true, "");
Check("whitespace", true, " \t\r\n ");
Check("label", true, "F-22", "F-22 Raptor");
Check("case insensitive", true, "rapTOR", "F-22 Raptor");
Check("trim", true, "  Raptor  ", "F-22 Raptor");
Check("workspace", true, "fleet", workspace: "Aircraft Fleet");
Check("vehicle", true, "shuttle", vehicle: "Space Shuttle");
Check("filename", true, "mig", file: "MiG-29.json");
Check("localized label", true, "火箭", "测试火箭");
Check("unicode", true, "火箭", vehicle: "测试火箭");
Check("no match", false, "747", "F-22", "Fighters", "MiG-29", "fighter.json");
Check("null values", false, "test");
Check("empty values", false, "test", "", "", "", "");
Check("literal wildcard", false, "*", "Raptor");
Check("literal wildcard present", true, "*", "Craft * prototype");
Check("literal bracket", true, "[Mk2]", "Shuttle [Mk2]");
Check("middle substring", true, "uttle", "Shuttle");
Check("substring not token split", false, "space plane", "Space", vehicle: "Plane");
Check("repeated spaces literal", false, "space  plane", "Space Plane");
Check("clear restores craft", true, "", "F-22");
Check("name mismatch still filename match", true, "osprey", "Transport", file: "osprey.json");
Console.WriteLine($"{passed} craft-query tests passed.");

int corners = 0;
void CornerCheck(bool condition, string name)
{
    if (!condition) throw new Exception($"Pixel corner: {name}");
    corners++;
}
foreach (bool inner in new[] { false, true })
{
    int size = PixelCornerMask.Size;
    for (int y = 0; y < size; y++)
    for (int x = 0; x < size; x++)
    {
        bool filled = PixelCornerMask.IsFilled(x, y, inner);
        CornerCheck(filled == PixelCornerMask.IsFilled(size - 1 - x, y, inner), "horizontal symmetry");
        CornerCheck(filled == PixelCornerMask.IsFilled(x, size - 1 - y, inner), "vertical symmetry");
        CornerCheck(filled == PixelCornerMask.IsFilled(y, x, inner), "rotational corner symmetry");
        if (x >= 4 && x < size - 4) CornerCheck(filled, "solid sliced center");
    }
    CornerCheck(!PixelCornerMask.IsFilled(-1, 0, inner), "out of bounds");
    CornerCheck(!PixelCornerMask.IsFilled(size, 0, inner), "out of bounds");
}
CornerCheck(!PixelCornerMask.IsFilled(0, 0, false), "transparent corner");
CornerCheck(!PixelCornerMask.IsFilled(2, 0, false), "three-pixel top notch");
CornerCheck(PixelCornerMask.IsFilled(3, 0, false), "top edge begins at three");
CornerCheck(!PixelCornerMask.IsFilled(0, 1, false), "one-pixel second step");
CornerCheck(PixelCornerMask.IsFilled(1, 1, false), "second step begins at one");
CornerCheck(!PixelCornerMask.IsFilled(0, 2, false), "second step has two-pixel height");
CornerCheck(PixelCornerMask.IsFilled(0, 3, false), "straight side after corner");
CornerCheck(!PixelCornerMask.IsFilled(1, 0, true), "inner corner notch");
CornerCheck(PixelCornerMask.IsFilled(2, 0, true), "inner edge begins at two");
Console.WriteLine($"{corners} pixel-corner checks passed.");
