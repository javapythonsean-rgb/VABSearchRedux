using UnityEngine;
using UnityEngine.UI;

namespace VABSearchRedux;

// Colors sampled from the installed widget_load prefab, not stock-file edits.
internal static class CraftSearchTheme
{
    internal const float BarHeight = 56f;
    internal const float BarGap = 12f;
    internal const float OuterPadding = 12f;
    internal const float VerticalPadding = 8f;
    internal const float TextPadding = 16f;
    internal const float CountWidth = 116f;
    internal const float ControlGap = 12f;
    internal static readonly Color Fill = new Color32(34, 38, 46, 255);
    internal static readonly Color Border = new Color32(79, 86, 101, 255);
    internal static readonly Color MutedText = new Color32(170, 178, 204, 255);
    internal static readonly Color Text = new Color32(242, 242, 242, 255);
    internal static readonly Color Accent = new Color32(0, 209, 255, 255);

    internal static ColorBlock InputColors => new ColorBlock
    {
        normalColor = Border,
        highlightedColor = new Color32(122, 133, 153, 255),
        pressedColor = Accent,
        selectedColor = Accent,
        disabledColor = new Color32(55, 60, 70, 255),
        colorMultiplier = 1f,
        fadeDuration = 0.08f
    };

    internal static Sprite MakeCornerSprite(bool inner)
    {
        int size = PixelCornerMask.Size;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = inner ? "Craft Search Pixel Fill" : "Craft Search Pixel Frame",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            pixels[y * size + x] = PixelCornerMask.IsFilled(x, y, inner)
                ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        float border = inner ? 3f : 4f;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    internal static void ReleaseSprite(Sprite? sprite)
    {
        if (sprite == null) return;
        UnityEngine.Object.Destroy(sprite.texture);
        UnityEngine.Object.Destroy(sprite);
    }
}
