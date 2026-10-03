namespace VABSearchRedux;

internal static class CraftQuery
{
    internal static bool Matches(string? query, string? label, string? workspaceName,
        string? vehicleName, string? fileName)
    {
        var trimmed = query?.Trim();
        return string.IsNullOrEmpty(trimmed)
            || Contains(label, trimmed!) || Contains(workspaceName, trimmed!)
            || Contains(vehicleName, trimmed!) || Contains(fileName, trimmed!);
    }

    private static bool Contains(string? value, string query)
        => value?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
}
