using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Shared builder helpers for the editor UI generators (main menu, game over, …). Centralises the
/// frontend palette and the uGUI/TMP construction so the generated screens stay visually consistent.
/// </summary>
public static class MenuUIFactory
{
    // Frontend palette
    public static readonly Color Bg = new Color(0.055f, 0.067f, 0.086f, 1f);     // #0e1116
    public static readonly Color Panel = new Color(0.086f, 0.106f, 0.133f, 1f);  // #161b22
    public static readonly Color Raised = new Color(0.114f, 0.141f, 0.176f, 1f); // #1d242d
    public static readonly Color Line = new Color(0.165f, 0.192f, 0.231f, 1f);   // #2a313b
    public static readonly Color Accent = new Color(0.369f, 0.737f, 0.482f, 1f); // #5ebc7b
    public static readonly Color Loss = new Color(0.933f, 0.412f, 0.318f, 1f);   // #ee6951
    public static readonly Color OnAccent = new Color(0.039f, 0.051f, 0.063f, 1f); // #0a0d10
    public static readonly Color Text = new Color(0.906f, 0.925f, 0.937f, 1f);   // #e7ecef
    public static readonly Color Muted = new Color(0.596f, 0.631f, 0.678f, 1f);  // #98a1ad
    public static readonly Color Dim = new Color(0.42f, 0.447f, 0.502f, 1f);     // #6b7280

    #region Rect / layout primitives
    public static RectTransform NewRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void ConfigureLayout(HorizontalOrVerticalLayoutGroup g, float spacing, TextAnchor align)
    {
        g.spacing = spacing;
        g.childAlignment = align;
        g.childControlWidth = true;
        g.childControlHeight = true;
        g.childForceExpandWidth = false;
        g.childForceExpandHeight = false;
    }

    public static void AddLayoutElement(GameObject go, float w, float h)
    {
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        le.preferredHeight = h;
        le.minWidth = w;
        le.minHeight = h;
    }

    public static void Spacer(Transform parent, float height)
    {
        RectTransform rt = NewRect("Spacer", parent);
        AddLayoutElement(rt.gameObject, 10, height);
    }
    #endregion

    #region Containers
    public static GameObject CreatePanel(string name, Transform parent, Color bg, float spacing)
    {
        RectTransform rt = NewRect(name, parent);
        Stretch(rt);
        rt.gameObject.AddComponent<Image>().color = bg;
        VerticalLayoutGroup vlg = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureLayout(vlg, spacing, TextAnchor.MiddleCenter);
        return rt.gameObject;
    }

    /// <summary>Bordered card: returns the inner content transform (a padded vertical layout).</summary>
    public static RectTransform CreateCard(string name, Transform parent, float width, float height, float spacing)
    {
        RectTransform outer = NewRect(name, parent);
        outer.gameObject.AddComponent<Image>().color = Line; // 1px border
        AddLayoutElement(outer.gameObject, width, height);

        RectTransform inner = NewRect("Inner", outer);
        Stretch(inner);
        inner.offsetMin = new Vector2(1, 1);
        inner.offsetMax = new Vector2(-1, -1);
        inner.gameObject.AddComponent<Image>().color = Panel;
        VerticalLayoutGroup vlg = inner.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureLayout(vlg, spacing, TextAnchor.UpperCenter);
        vlg.padding = new RectOffset(30, 30, 26, 26);
        return inner;
    }

    /// <summary>Anchored bordered box with a vertical content layout; returns the inner content transform.</summary>
    public static RectTransform CreateAnchoredBox(string name, Transform parent, Vector2 anchor, Vector2 pivot,
        Vector2 pos, Vector2 size, TextAnchor align, float spacing = 2f)
    {
        RectTransform outer = NewRect(name, parent);
        outer.anchorMin = outer.anchorMax = anchor;
        outer.pivot = pivot;
        outer.sizeDelta = size;
        outer.anchoredPosition = pos;
        outer.gameObject.AddComponent<Image>().color = Line; // 1px border

        RectTransform inner = NewRect("Inner", outer);
        Stretch(inner);
        inner.offsetMin = new Vector2(1, 1);
        inner.offsetMax = new Vector2(-1, -1);
        inner.gameObject.AddComponent<Image>().color = new Color(Panel.r, Panel.g, Panel.b, 0.92f);

        VerticalLayoutGroup vlg = inner.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureLayout(vlg, spacing, align);
        vlg.padding = new RectOffset(14, 14, 8, 8);
        return inner;
    }

    /// <summary>Circular radial gauge (dark disc track + accent fill). Returns the radial fill Image,
    /// whose <c>fillAmount</c> (0–1) fills the circle clockwise from the top.</summary>
    public static Image CreateRadialGauge(Transform parent, float size)
    {
        Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        RectTransform track = NewRect("GaugeTrack", parent);
        AddLayoutElement(track.gameObject, size, size);
        Image trackImg = track.gameObject.AddComponent<Image>();
        trackImg.sprite = circle;
        trackImg.color = Line;

        RectTransform fillRt = NewRect("Fill", track);
        Stretch(fillRt);
        Image fill = fillRt.gameObject.AddComponent<Image>();
        fill.sprite = circle;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Radial360;
        fill.fillOrigin = (int)Image.Origin360.Top;
        fill.fillClockwise = true;
        fill.color = Accent;
        fill.fillAmount = 0f;
        return fill;
    }

    /// <summary>Horizontal progress bar (dark track + accent fill). Returns the filled fill Image,
    /// whose <c>fillAmount</c> (0–1) drives the bar.</summary>
    public static Image CreateProgressBar(Transform parent, float width, float height)
    {
        RectTransform track = NewRect("Track", parent);
        AddLayoutElement(track.gameObject, width, height);
        track.gameObject.AddComponent<Image>().color = Bg;

        RectTransform fillRt = NewRect("Fill", track);
        Stretch(fillRt);
        Image fill = fillRt.gameObject.AddComponent<Image>();
        fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = Accent;
        fill.fillAmount = 0f;
        return fill;
    }

    public static void CreateBrandMark(Transform parent)
    {
        RectTransform outer = NewRect("Brand", parent);
        AddLayoutElement(outer.gameObject, 28, 28);
        outer.gameObject.AddComponent<Image>().color = Text; // white ring

        RectTransform inner = NewRect("Inner", outer);
        Stretch(inner);
        inner.offsetMin = new Vector2(2, 2);
        inner.offsetMax = new Vector2(-2, -2);
        inner.gameObject.AddComponent<Image>().color = Bg;

        RectTransform dot = NewRect("Dot", inner);
        dot.anchorMin = dot.anchorMax = new Vector2(0.5f, 0.5f);
        dot.sizeDelta = new Vector2(7, 7);
        dot.anchoredPosition = Vector2.zero;
        dot.gameObject.AddComponent<Image>().color = Accent;
    }
    #endregion

    #region Text / controls
    public static TextMeshProUGUI CreateLabel(string name, string text, Transform parent, int size, Color color,
        float w, float h, bool bold, bool upper, float spacing, TextAlignmentOptions align)
    {
        RectTransform rt = NewRect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.characterSpacing = spacing;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;
        if (bold) t.fontStyle |= FontStyles.Bold;
        if (upper) t.fontStyle |= FontStyles.UpperCase;
        AddLayoutElement(rt.gameObject, w, h);
        return t;
    }

    public static Button CreateButton(string name, string label, Transform parent, bool primary, float width = 300)
    {
        RectTransform rt = NewRect(name, parent);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = primary ? Accent : Line; // outline = border colour, filled by inner
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = bg;
        AddLayoutElement(rt.gameObject, width, 50);

        Transform labelParent = rt;
        if (!primary)
        {
            RectTransform inner = NewRect("Inner", rt);
            Stretch(inner);
            inner.offsetMin = new Vector2(1, 1);
            inner.offsetMax = new Vector2(-1, -1);
            inner.gameObject.AddComponent<Image>().color = Raised;
            labelParent = inner;
        }

        RectTransform lrt = NewRect("Label", labelParent);
        Stretch(lrt);
        TextMeshProUGUI t = lrt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = label;
        t.fontSize = 18;
        t.color = primary ? OnAccent : Text;
        t.alignment = TextAlignmentOptions.Center;
        t.characterSpacing = 2f;
        t.fontStyle = FontStyles.UpperCase | FontStyles.Bold;
        return btn;
    }

    public static TMP_InputField CreateInputField(string name, Transform parent)
    {
        RectTransform rt = NewRect(name, parent);
        AddLayoutElement(rt.gameObject, 180, 38);
        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = Bg;
        TMP_InputField input = rt.gameObject.AddComponent<TMP_InputField>();

        RectTransform area = NewRect("Text Area", rt);
        Stretch(area);
        area.offsetMin = new Vector2(10, 6);
        area.offsetMax = new Vector2(-10, -6);
        area.gameObject.AddComponent<RectMask2D>();

        RectTransform textRT = NewRect("Text", area);
        Stretch(textRT);
        TextMeshProUGUI textComp = textRT.gameObject.AddComponent<TextMeshProUGUI>();
        textComp.fontSize = 17;
        textComp.color = Text;
        textComp.alignment = TextAlignmentOptions.MidlineLeft;

        input.textViewport = area;
        input.textComponent = textComp;
        input.fontAsset = textComp.font;
        input.targetGraphic = bg;
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        return input;
    }

    /// <summary>A "label + input" row; returns the input field.</summary>
    public static TMP_InputField CreateSpecRow(string name, string label, Transform parent)
    {
        RectTransform row = NewRect(name + "Row", parent);
        AddLayoutElement(row.gameObject, 600, 40);
        ConfigureLayout(row.gameObject.AddComponent<HorizontalLayoutGroup>(), 16f, TextAnchor.MiddleLeft);

        RectTransform lrt = NewRect("Label", row);
        TextMeshProUGUI lt = lrt.gameObject.AddComponent<TextMeshProUGUI>();
        lt.text = label;
        lt.fontSize = 17;
        lt.color = Muted;
        lt.alignment = TextAlignmentOptions.Left;
        AddLayoutElement(lrt.gameObject, 390, 36);

        return CreateInputField("Input", row);
    }

    /// <summary>A "label + slider" row; returns the slider.</summary>
    public static Slider CreateSliderRow(string name, string label, Transform parent)
    {
        RectTransform row = NewRect(name + "Row", parent);
        AddLayoutElement(row.gameObject, 600, 40);
        ConfigureLayout(row.gameObject.AddComponent<HorizontalLayoutGroup>(), 16f, TextAnchor.MiddleLeft);

        RectTransform lrt = NewRect("Label", row);
        TextMeshProUGUI lt = lrt.gameObject.AddComponent<TextMeshProUGUI>();
        lt.text = label;
        lt.fontSize = 17;
        lt.color = Muted;
        lt.alignment = TextAlignmentOptions.Left;
        AddLayoutElement(lrt.gameObject, 390, 36);

        RectTransform rt = NewRect("Slider", row);
        AddLayoutElement(rt.gameObject, 180, 20);
        Slider slider = rt.gameObject.AddComponent<Slider>();

        RectTransform bg = NewRect("Background", rt);
        Stretch(bg);
        bg.gameObject.AddComponent<Image>().color = Bg;

        RectTransform fillArea = NewRect("Fill Area", rt);
        Stretch(fillArea);
        RectTransform fill = NewRect("Fill", fillArea);
        fill.sizeDelta = new Vector2(10, 0);
        fill.gameObject.AddComponent<Image>().color = Accent;

        RectTransform handleArea = NewRect("Handle Slide Area", rt);
        Stretch(handleArea);
        RectTransform handle = NewRect("Handle", handleArea);
        handle.sizeDelta = new Vector2(16, 16);
        Image handleImg = handle.gameObject.AddComponent<Image>();
        handleImg.color = Text;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImg;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }
    #endregion

    #region Editor utilities
    public static void SetRef(SerializedObject so, string property, UnityEngine.Object value)
    {
        SerializedProperty p = so.FindProperty(property);
        if (p != null) p.objectReferenceValue = value;
        else Debug.LogWarning("[MenuUIFactory] Missing serialized property: " + property);
    }

    public static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
    }

    public static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindObjectOfType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
        Type inputModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (inputModule != null) es.AddComponent(inputModule);
        else es.AddComponent<StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }
    #endregion
}
