using System.Reflection;
using HarmonyLib;
using KSP.OAB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VABSearchRedux;

internal sealed class CraftSearchController : MonoBehaviour
{
    private static readonly FieldInfo SelectedEntry = AccessTools.Field(typeof(OABLoadDialog), "_selectedEntry");
    private static readonly FieldInfo Metadata = AccessTools.Field(typeof(OABLoadDialogExistingSave), "_workspaceMetaInfo");
    private OABLoadDialog _dialog = null!;
    private List<OABLoadDialogExistingSave> _entries = null!;
    private TMP_InputField? _field;
    private TMP_Text? _count;
    private ScrollRect? _scroll;
    private RectTransform? _viewport;
    private GameObject? _bar;
    private Sprite? _frameSprite;
    private Sprite? _fillSprite;
    private Vector2 _originalViewportMax;
    private ScrollRect.ScrollbarVisibility _originalVerticalVisibility;
    private ScrollRect.ScrollbarVisibility _originalHorizontalVisibility;
    private bool _layoutChanged;
    private bool _dirty;
    private bool _resetScroll;
    private bool _building;

    internal void Open(OABLoadDialog dialog, Transform entryParent,
        List<OABLoadDialogExistingSave> entries)
    {
        _dialog = dialog;
        _entries = entries;
        if (_field != null)
            _field.SetTextWithoutNotify(string.Empty);
        Refresh(true);
        if (_field == null && !_building)
            StartCoroutine(BuildWhenReady(entryParent));
    }

    internal void Refresh(bool resetScroll = false)
    {
        _dirty = true;
        _resetScroll |= resetScroll;
    }

    private System.Collections.IEnumerator BuildWhenReady(Transform entryParent)
    {
        _building = true;
        yield return null; // Let the game's bindings/layout settle first.
        try
        {
            _scroll = entryParent.GetComponentInParent<ScrollRect>();
            if (_scroll == null || _scroll.viewport == null || _scroll.content != entryParent)
                throw new InvalidOperationException("Craft-list ScrollRect/viewport was not found");

            var dialogTexts = _dialog.GetComponentsInChildren<TMP_Text>(true);
            // Prefer the sort controls' regular UI font over the small window
            // title's display font. Both come from the game's own dialog.
            var sample = dialogTexts.FirstOrDefault(text => text.font != null
                && text.fontSize >= 14f && text.fontSize <= 20f
                && text.GetComponentInParent<Toggle>() != null);
            var font = sample?.font ?? dialogTexts.Select(text => text.font)
                .FirstOrDefault(asset => asset != null) ?? TMP_Settings.defaultFontAsset;
            if (font == null)
                throw new InvalidOperationException("Dialog font was not found");

            _viewport = _scroll.viewport;
            _originalViewportMax = _viewport.offsetMax;
            _originalVerticalVisibility = _scroll.verticalScrollbarVisibility;
            _originalHorizontalVisibility = _scroll.horizontalScrollbarVisibility;
            _layoutChanged = true;
            // AutoHideAndExpandViewport overwrites viewport offsets on every
            // layout rebuild. Keep auto-hide, but retain our reserved top space.
            if (_originalVerticalVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                _scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            if (_originalHorizontalVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport)
                _scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            _bar = new GameObject("VAB Craft Search", typeof(RectTransform));
            var barRect = (RectTransform)_bar.transform;
            barRect.SetParent(_viewport.parent, false);
            // Use the viewport's original top edge, then reserve space beneath
            // the search bar. No dialog buttons or sorting controls are moved.
            barRect.anchorMin = new Vector2(_viewport.anchorMin.x, _viewport.anchorMax.y);
            barRect.anchorMax = _viewport.anchorMax;
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.offsetMin = new Vector2(_viewport.offsetMin.x,
                _originalViewportMax.y - CraftSearchTheme.BarHeight);
            barRect.offsetMax = _originalViewportMax;
            _bar.AddComponent<LayoutElement>().ignoreLayout = true;
            _viewport.offsetMax = _originalViewportMax
                - new Vector2(0f, CraftSearchTheme.BarHeight + CraftSearchTheme.BarGap);

            _frameSprite = CraftSearchTheme.MakeCornerSprite(false);
            _fillSprite = CraftSearchTheme.MakeCornerSprite(true);

            var fieldObject = new GameObject("Search crafts", typeof(RectTransform), typeof(Image));
            var fieldRect = (RectTransform)fieldObject.transform;
            fieldRect.SetParent(barRect, false);
            Stretch(fieldRect, new Vector2(CraftSearchTheme.OuterPadding, CraftSearchTheme.VerticalPadding),
                new Vector2(-(CraftSearchTheme.OuterPadding + CraftSearchTheme.CountWidth
                    + CraftSearchTheme.ControlGap), -CraftSearchTheme.VerticalPadding));
            var background = fieldObject.GetComponent<Image>();
            background.sprite = _frameSprite;
            background.type = Image.Type.Sliced;
            // ColorTint supplies the border colors; keep the graphic itself
            // white so those tints are not multiplied by the base color.
            background.color = Color.white;
            MakeFill(fieldRect);

            var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            var textRect = (RectTransform)textArea.transform;
            textRect.SetParent(fieldRect, false);
            Stretch(textRect, new Vector2(CraftSearchTheme.TextPadding, 8f),
                new Vector2(-CraftSearchTheme.TextPadding, -8f));
            var text = MakeText("Text", textRect, font);
            var hint = MakeText("Placeholder", textRect, font);
            hint.text = "Search crafts... (Ctrl+F)";
            hint.color = CraftSearchTheme.MutedText;

            _field = fieldObject.AddComponent<TMP_InputField>();
            _field.targetGraphic = background;
            _field.transition = Selectable.Transition.ColorTint;
            _field.colors = CraftSearchTheme.InputColors;
            _field.textViewport = textRect;
            _field.textComponent = text;
            _field.placeholder = hint;
            _field.fontAsset = font;
            _field.lineType = TMP_InputField.LineType.SingleLine;
            _field.characterLimit = 128;
            _field.customCaretColor = true;
            _field.caretColor = CraftSearchTheme.Accent;
            _field.restoreOriginalTextOnEscape = false;
            _field.onValueChanged.AddListener(_ => Refresh(true));
            _field.onEndEdit.AddListener(_ =>
            {
                // EventSystem can process Escape before this component.Update.
                if (_field != null && _field.wasCanceled) ClearSearch();
            });

            var countObject = new GameObject("Craft count", typeof(RectTransform), typeof(Image));
            var countRect = (RectTransform)countObject.transform;
            countRect.SetParent(barRect, false);
            countRect.anchorMin = new Vector2(1f, 0f);
            countRect.anchorMax = Vector2.one;
            countRect.offsetMin = new Vector2(-(CraftSearchTheme.OuterPadding + CraftSearchTheme.CountWidth),
                CraftSearchTheme.VerticalPadding);
            countRect.offsetMax = new Vector2(-CraftSearchTheme.OuterPadding, -CraftSearchTheme.VerticalPadding);
            var countFrame = countObject.GetComponent<Image>();
            countFrame.sprite = _frameSprite;
            countFrame.type = Image.Type.Sliced;
            countFrame.color = CraftSearchTheme.Border;
            countFrame.raycastTarget = false;
            MakeFill(countRect);
            _count = MakeText("Matches", countRect, font);
            _count.fontSize = 16f;
            _count.alignment = TextAlignmentOptions.Center;
            _count.color = CraftSearchTheme.MutedText;
            Stretch(_count.rectTransform, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            Refresh(true);
            Debug.Log("[VAB Search Redux] Craft-load search field created");
        }
        catch (Exception exception)
        {
            RestoreLayout();
            if (_bar != null) Destroy(_bar);
            ReleaseTheme();
            _field = null;
            _count = null;
            Debug.LogError($"[VAB Search Redux] Could not create craft search: {exception}");
        }
        finally
        {
            _building = false;
        }
    }

    private static TextMeshProUGUI MakeText(string name, Transform parent, TMP_FontAsset font)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        var text = obj.AddComponent<TextMeshProUGUI>();
        Stretch(text.rectTransform, Vector2.zero, Vector2.zero);
        text.font = font;
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        text.color = CraftSearchTheme.Text;
        text.richText = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private void MakeFill(RectTransform parent)
    {
        var obj = new GameObject("Inset fill", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)obj.transform;
        rect.SetParent(parent, false);
        Stretch(rect, Vector2.one, -Vector2.one);
        var image = obj.GetComponent<Image>();
        image.sprite = _fillSprite;
        image.type = Image.Type.Sliced;
        image.color = CraftSearchTheme.Fill;
        image.raycastTarget = false;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = min;
        rect.offsetMax = max;
    }

    private void Update()
    {
        if (_field == null) return;
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        if (ctrl && Input.GetKeyDown(KeyCode.F))
        {
            _field.Select();
            _field.ActivateInputField();
        }
        if (_field.isFocused && Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSearch();
            _field.DeactivateInputField();
        }
    }

    private void ClearSearch()
    {
        _field?.SetTextWithoutNotify(string.Empty);
        Refresh(true);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    private void LateUpdate()
    {
        if (!_dirty || _field == null || _entries == null) return;
        _dirty = false;
        int visible = 0;
        int total = 0;
        var selected = SelectedEntry.GetValue(_dialog) as OABLoadDialogExistingSave;
        foreach (var entry in _entries)
        {
            if (entry == null) continue;
            total++;
            var metadata = Metadata.GetValue(entry) as OABSavedWorkspaceMetaInfo;
            bool matches = CraftQuery.Matches(_field.text, entry.Label, metadata?.Name,
                metadata?.VehicleName, entry.FileName);
            if (!matches && entry == selected)
            {
                var toggle = entry.GetComponent<Toggle>();
                if (toggle != null)
                {
                    // A group that disallows switching off otherwise refuses to
                    // clear its last toggle. Avoid firing row actions here.
                    var group = toggle.group;
                    bool allowSwitchOff = group != null && group.allowSwitchOff;
                    if (group != null) group.allowSwitchOff = true;
                    try { toggle.SetIsOnWithoutNotify(false); }
                    finally { if (group != null) group.allowSwitchOff = allowSwitchOff; }
                }
                _dialog.SelectExistingSaveEntry(null!, false);
            }
            if (entry.gameObject.activeSelf != matches) entry.gameObject.SetActive(matches);
            if (matches) visible++;
        }
        if (_count != null) _count.text = visible == 0 ? "No matches" : $"{visible} / {total}";
        if (_resetScroll && _scroll != null)
        {
            Canvas.ForceUpdateCanvases();
            _scroll.StopMovement();
            _scroll.verticalNormalizedPosition = 1f;
        }
        _resetScroll = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _building = false;
        if (_field != null)
        {
            _field.DeactivateInputField();
            _field.SetTextWithoutNotify(string.Empty);
        }
    }

    private void RestoreLayout()
    {
        if (!_layoutChanged) return;
        if (_viewport != null) _viewport.offsetMax = _originalViewportMax;
        if (_scroll != null)
        {
            _scroll.verticalScrollbarVisibility = _originalVerticalVisibility;
            _scroll.horizontalScrollbarVisibility = _originalHorizontalVisibility;
        }
        _layoutChanged = false;
    }

    private void OnDestroy()
    {
        RestoreLayout();
        if (_bar != null) Destroy(_bar);
        ReleaseTheme();
    }

    private void ReleaseTheme()
    {
        CraftSearchTheme.ReleaseSprite(_frameSprite);
        CraftSearchTheme.ReleaseSprite(_fillSprite);
        _frameSprite = null;
        _fillSprite = null;
    }
}
