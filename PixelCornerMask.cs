namespace VABSearchRedux;

// Binary alpha, symmetric under reflections and 90-degree rotations.
// The 3/1/1/0 corner steps remain
// unscaled when used as the fixed corners of a point-filtered sliced sprite.
internal static class PixelCornerMask
{
    internal const int Size = 12;

    internal static bool IsFilled(int x, int y, bool inner)
    {
        if (x < 0 || y < 0 || x >= Size || y >= Size) return false;
        int edgeY = Math.Min(y, Size - 1 - y);
        int inset = inner ? (edgeY == 0 ? 2 : edgeY == 1 ? 1 : 0)
            : (edgeY == 0 ? 3 : edgeY < 3 ? 1 : 0);
        return x >= inset && x < Size - inset;
    }
}
