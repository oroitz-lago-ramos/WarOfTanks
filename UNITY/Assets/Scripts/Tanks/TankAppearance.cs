using UnityEngine;
using WarOfTanks.Enums;

/// <summary>
/// Gives the tank its look entirely in code: a dark square body with a coloured
/// border plus a thin cannon line, tinted by team (PLAYER = green, ENEMY = red).
///
/// The body sprite is generated with a white border and a dark-grey fill; tinting
/// it by the team colour yields a bright coloured outline and a dark coloured
/// interior from a single sprite. Body and cannon stay separate objects so the
/// cannon (turret) can rotate independently.
///
/// Add this to the Tank prefab root (it reads <see cref="Tank.TeamId"/>). Runs in
/// the editor too for a live preview.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Tank))]
public class TankAppearance : MonoBehaviour
{
    [Header("Renderers (auto-found by child name if empty)")]
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private SpriteRenderer _cannonRenderer;

    [Header("Team Colors")]
    [SerializeField] private Color _playerColor = new Color(0.40f, 0.85f, 0.45f);
    [SerializeField] private Color _enemyColor  = new Color(0.90f, 0.32f, 0.30f);

    [Header("Body Style")]
    [Tooltip("Border thickness as a fraction of the square's size.")]
    [Range(0.02f, 0.45f)][SerializeField] private float _borderThickness = 0.12f;
    [Tooltip("Grey value of the interior before tinting (0 = black, 1 = white).")]
    [Range(0f, 1f)][SerializeField] private float _fillDarkness = 0.20f;

    [Header("Emissive Glow (additive, WebGL-safe)")]
    [SerializeField] private bool _enableGlow = true;
    [Tooltip("Glow size relative to the tank body (1 = body size).")]
    [Range(1f, 4f)][SerializeField] private float _glowScale = 2.2f;
    [Tooltip("Brightness of the glow. Boosts past the team colour without needing HDR.")]
    [Range(0f, 4f)][SerializeField] private float _glowIntensity = 1.3f;

    // Shared across all tanks; rebuilt only when the body style changes.
    private static Sprite _bodySprite;
    private static Sprite _cannonSprite;
    private static Sprite _glowSprite;
    private static Material _glowMaterial;
    private static float _bakedBorder = -1f;
    private static float _bakedFill = -1f;

    private const string GlowChildName = "TankGlow";
    private bool _allowCreate;

    private void OnEnable()
    {
        _allowCreate = true;   // creating child GameObjects is safe here (not in OnValidate)
        Apply();
        _allowCreate = false;
    }

    private void OnValidate() => Apply();

    /// <summary>Builds/assigns the sprites and applies the team colour.</summary>
    public void Apply()
    {
        ResolveRenderers();
        EnsureSprites();

        Color team = GetTeamColor();
        AssignIfChanged(_bodyRenderer, _bodySprite, team);
        AssignIfChanged(_cannonRenderer, _cannonSprite, team);

        FixCannonPivot();
        SetupGlow(team);
    }

    /// <summary>
    /// Adds/updates a soft additive glow aura behind the tank, tinted by team.
    /// Additive sprite blending works on WebGL (no HDR / post-processing needed).
    /// The aura is centered on the body, so it reads correctly at any cannon angle.
    /// </summary>
    private void SetupGlow(Color team)
    {
        // Parent the glow under the body so it hides/shows with it: Tank.Die()
        // deactivates the body, and a child glow follows automatically (it used to
        // sit under the Tank root, which death never toggled).
        Transform parent = _bodyRenderer != null ? _bodyRenderer.transform : transform;

        Transform glow = parent.Find(GlowChildName);
        if (glow == null) glow = transform.Find(GlowChildName); // legacy: was under the Tank root

        if (!_enableGlow)
        {
            if (glow != null) glow.gameObject.SetActive(false);
            return;
        }

        if (glow == null)
        {
            if (!_allowCreate) return;            // wait for OnEnable to create it
            glow = new GameObject(GlowChildName).transform;
            glow.gameObject.AddComponent<SpriteRenderer>();
            glow.SetParent(parent, false);
        }
        else if (glow.parent != parent)
        {
            if (!_allowCreate) return;            // migrate during OnEnable, not OnValidate
            glow.SetParent(parent, false);
        }
        glow.gameObject.SetActive(true);
        glow.localPosition = Vector3.zero;
        glow.localRotation = Quaternion.identity;
        glow.localScale = new Vector3(_glowScale, _glowScale, 1f);

        var gr = glow.GetComponent<SpriteRenderer>();
        gr.sprite = _glowSprite;
        gr.color = team;
        if (_glowMaterial != null)
        {
            if (gr.sharedMaterial != _glowMaterial) gr.sharedMaterial = _glowMaterial;
            // Shared material: intensity is global. Only write when it actually changes.
            if (!Mathf.Approximately(_glowMaterial.GetFloat("_Intensity"), _glowIntensity))
                _glowMaterial.SetFloat("_Intensity", _glowIntensity);
        }

        // Draw behind the body so the dark body sits over the glow's center, leaving a halo.
        if (_bodyRenderer != null)
        {
            gr.sortingLayerID = _bodyRenderer.sortingLayerID;
            gr.sortingOrder = _bodyRenderer.sortingOrder - 1;
        }
    }

    /// <summary>
    /// Makes the cannon pivot from the tank centre. The cannon transform (which the
    /// TurretController rotates) is moved to the body centre, and the cannon sprite
    /// uses a left-edge pivot so the barrel extends outward instead of swinging on an
    /// off-centre arc. The CannonTip (muzzle) is placed at the barrel's far end.
    /// </summary>
    private void FixCannonPivot()
    {
        if (_cannonRenderer == null) return;
        Transform cannon = _cannonRenderer.transform;

        if (cannon.localPosition != Vector3.zero)
            cannon.localPosition = Vector3.zero;

        // Render the barrel above the body (they now overlap at the centre).
        if (_bodyRenderer != null && _cannonRenderer.sortingOrder <= _bodyRenderer.sortingOrder)
            _cannonRenderer.sortingOrder = _bodyRenderer.sortingOrder + 1;

        // Sprite spans local x 0..1 from the pivot, so the muzzle is at local x = 1.
        Transform tip = cannon.Find("CannonTip");
        var muzzle = new Vector3(1f, 0f, 0f);
        if (tip != null && tip.localPosition != muzzle)
            tip.localPosition = muzzle;
    }

    private Color GetTeamColor()
    {
        var tank = GetComponent<Tank>();
        bool enemy = tank != null && tank.TeamId == ETankTeam.ENEMY;
        return enemy ? _enemyColor : _playerColor;
    }

    private void ResolveRenderers()
    {
        if (_bodyRenderer == null)
        {
            var t = transform.Find("TankBody");
            if (t != null) _bodyRenderer = t.GetComponent<SpriteRenderer>();
        }
        if (_cannonRenderer == null)
        {
            var t = transform.Find("Cannon");
            if (t != null) _cannonRenderer = t.GetComponent<SpriteRenderer>();
        }
    }

    private static void AssignIfChanged(SpriteRenderer r, Sprite sprite, Color color)
    {
        if (r == null) return;
        if (r.sprite != sprite) r.sprite = sprite;
        if (r.color != color) r.color = color;
    }

    private void EnsureSprites()
    {
        if (_bodySprite == null ||
            !Mathf.Approximately(_bakedBorder, _borderThickness) ||
            !Mathf.Approximately(_bakedFill, _fillDarkness))
        {
            _bodySprite = BuildOutlinedSquare(64, _borderThickness, _fillDarkness);
            _bakedBorder = _borderThickness;
            _bakedFill = _fillDarkness;
        }
        if (_cannonSprite == null)
            _cannonSprite = BuildSolid(8);
        if (_glowSprite == null)
            _glowSprite = BuildRadialGlow(128);
        if (_glowMaterial == null)
        {
            // Loaded from Resources so the additive shader is always included in builds
            // (WebGL strips shaders only reachable via Shader.Find).
            _glowMaterial = Resources.Load<Material>("TankGlowAdditive");
            if (_glowMaterial == null)
            {
                Shader shader = Shader.Find("WarOfTanks/TankGlowAdditive");
                if (shader != null)
                    _glowMaterial = new Material(shader) { name = "TankGlow (runtime)", hideFlags = HideFlags.DontSave };
            }
        }
    }

    /// <summary>Soft radial falloff stored in alpha; rgb white so the team tint colours it.</summary>
    private static Sprite BuildRadialGlow(int size)
    {
        var cols = new Color[size * size];
        float c = (size - 1) * 0.5f;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / r;
                float a = Mathf.Clamp01(1f - d);
                a = a * a * (3f - 2f * a); // smoothstep for a softer core/edge
                cols[y * size + x] = new Color(1f, 1f, 1f, a);
            }
        // PPU = size so the base sprite is one unit; the renderer scales it up.
        return MakeSprite(cols, size, "TankGlowSprite", new Vector2(0.5f, 0.5f));
    }

    private static Sprite BuildOutlinedSquare(int size, float borderFrac, float fill)
    {
        int b = Mathf.Max(1, Mathf.RoundToInt(size * borderFrac));
        var border = Color.white;
        var fillC = new Color(fill, fill, fill, 1f);
        var cols = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x < b || x >= size - b || y < b || y >= size - b;
                cols[y * size + x] = isBorder ? border : fillC;
            }
        // Centered pivot: the body is centered on the tank.
        return MakeSprite(cols, size, "TankBodySprite", new Vector2(0.5f, 0.5f));
    }

    private static Sprite BuildSolid(int size)
    {
        var cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.white;
        // Left-edge pivot: the barrel base sits at the tank centre and extends outward.
        return MakeSprite(cols, size, "TankCannonSprite", new Vector2(0f, 0.5f));
    }

    private static Sprite MakeSprite(Color[] cols, int size, string name, Vector2 pivot)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            name = name,
            hideFlags = HideFlags.DontSave,
        };
        tex.SetPixels(cols);
        tex.Apply();

        // PPU = size so the sprite is exactly one world unit (matches a 1-unit body).
        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), pivot, size);
        sprite.name = name;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }
}
